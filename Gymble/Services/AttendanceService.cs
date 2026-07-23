using Dapper;
using Gymble.Models;
using Gymble.Repositories;
using System.Data.SQLite;

namespace Gymble.Services
{
    public interface IAttendanceService
    {
        Task<CheckInResult> CheckInAsync(long memberId, DateTime checkedInAt, CancellationToken ct = default);
        Task<IReadOnlyList<AttendanceViewItem>> GetByDateAsync(DateTime date, CancellationToken ct = default);
        Task<bool> HasCheckedInAsync(long memberId, DateTime date, CancellationToken ct = default);
    }

    public sealed class AttendanceService : IAttendanceService
    {
        private readonly Func<SQLiteConnection> _connFactory;
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IMembershipRepository _membershipRepository;

        public AttendanceService(
            Func<SQLiteConnection> connFactory,
            IAttendanceRepository attendanceRepository,
            IMembershipRepository membershipRepository)
        {
            _connFactory = connFactory ?? throw new ArgumentNullException(nameof(connFactory));
            _attendanceRepository = attendanceRepository ?? throw new ArgumentNullException(nameof(attendanceRepository));
            _membershipRepository = membershipRepository ?? throw new ArgumentNullException(nameof(membershipRepository));
        }

        public Task<IReadOnlyList<AttendanceViewItem>> GetByDateAsync(DateTime date, CancellationToken ct = default)
        {
            return _attendanceRepository.GetByDateAsync(ToCheckinDate(date), ct);
        }

        public Task<bool> HasCheckedInAsync(long memberId, DateTime date, CancellationToken ct = default)
        {
            return _attendanceRepository.HasCheckedInAsync(memberId, ToCheckinDate(date), ct);
        }

        public async Task<CheckInResult> CheckInAsync(long memberId, DateTime checkedInAt, CancellationToken ct = default)
        {
            var localCheckedInAt = checkedInAt == default ? DateTime.Now : checkedInAt;
            var attendanceDate = localCheckedInAt.Date;
            var checkinDate = ToCheckinDate(localCheckedInAt);

            using var conn = _connFactory();
            using var tx = conn.BeginTransaction();

            try
            {
                var member = await GetMemberAsync(conn, tx, memberId, ct);
                if (member == null)
                {
                    tx.Rollback();
                    return Fail(CheckInResultType.MemberNotFound, (int)memberId, "", localCheckedInAt, "회원을 찾을 수 없습니다.");
                }

                if (member.Status != MemberStatus.Active)
                {
                    tx.Rollback();
                    return Fail(CheckInResultType.MemberUnavailable, member.Id, member.Name ?? "", localCheckedInAt, "출석 가능한 회원 상태가 아닙니다.");
                }

                if (await _attendanceRepository.HasCheckedInAsync(conn, tx, memberId, checkinDate, ct))
                {
                    tx.Rollback();
                    return Fail(CheckInResultType.AlreadyCheckedIn, member.Id, member.Name ?? "", localCheckedInAt, "이미 오늘 출석한 회원입니다.");
                }

                var memberships = await _membershipRepository.GetByMemberIdAsync(conn, tx, memberId, ct);
                var selectedMembership = SelectGymMembership(memberships, attendanceDate);

                if (selectedMembership == null)
                {
                    tx.Rollback();
                    return Fail(CheckInResultType.NoUsableMembership, member.Id, member.Name ?? "", localCheckedInAt, "사용 가능한 헬스 이용권이 없습니다.");
                }

                var now = DateTime.Now;
                var usageResults = new List<MembershipUsageResult>();

                var shouldActivatePurchaseGroup = IsFirstCheckInPending(selectedMembership);
                if (shouldActivatePurchaseGroup)
                {
                    var relatedFirstCheckInMemberships = memberships
                        .Where(x => x.PurchaseId == selectedMembership.PurchaseId)
                        .Where(IsFirstCheckInPending)
                        .OrderBy(x => x.Id)
                        .ToList();

                    foreach (var membership in relatedFirstCheckInMemberships)
                    {
                        var before = Snapshot(membership);
                        ActivateFirstCheckInMembership(membership, localCheckedInAt, now);
                        await _membershipRepository.UpdateUsageAsync(conn, tx, membership, ct);
                        usageResults.Add(CreateUsageResult(before, membership, activated: true, decremented: false));
                    }
                }

                var beforeRemaining = selectedMembership.RemainingCount;

                if (selectedMembership.UsageType == ProductUsageType.Count)
                {
                    var before = Snapshot(selectedMembership);
                    DecrementCountMembership(selectedMembership, now);
                    await _membershipRepository.UpdateUsageAsync(conn, tx, selectedMembership, ct);
                    usageResults.Add(CreateUsageResult(before, selectedMembership, activated: false, decremented: true));
                }
                else if (!shouldActivatePurchaseGroup)
                {
                    usageResults.Add(CreateUsageResult(Snapshot(selectedMembership), selectedMembership, activated: false, decremented: false));
                }

                var attendance = new Attendance
                {
                    MemberId = member.Id,
                    CheckedInAt = localCheckedInAt,
                    CheckinDate = checkinDate,
                    MembershipId = selectedMembership.Id,
                    MembershipBeforeRemaining = beforeRemaining,
                    MembershipAfterRemaining = selectedMembership.RemainingCount
                };

                long attendanceId;
                try
                {
                    attendanceId = await _attendanceRepository.InsertAsync(conn, tx, attendance, ct);
                }
                catch (SQLiteException ex) when (ex.ResultCode == SQLiteErrorCode.Constraint)
                {
                    tx.Rollback();
                    return Fail(CheckInResultType.AlreadyCheckedIn, member.Id, member.Name ?? "", localCheckedInAt, "이미 오늘 출석한 회원입니다.");
                }

                tx.Commit();

                return new CheckInResult
                {
                    Success = true,
                    ResultType = CheckInResultType.Success,
                    AttendanceId = attendanceId,
                    MemberId = member.Id,
                    MemberName = member.Name ?? "",
                    CheckedInAt = localCheckedInAt,
                    MembershipResults = usageResults,
                    Message = "출석이 완료되었습니다."
                };
            }
            catch (Exception ex)
            {
                tx.Rollback();

                return new CheckInResult
                {
                    Success = false,
                    ResultType = CheckInResultType.Failed,
                    MemberId = (int)memberId,
                    CheckedInAt = localCheckedInAt,
                    Message = $"출석 처리 중 오류가 발생했습니다. {ex.Message}"
                };
            }
        }

        private static async Task<Member?> GetMemberAsync(
            SQLiteConnection conn,
            SQLiteTransaction tx,
            long memberId,
            CancellationToken ct)
        {
            const string sql = @"
                SELECT
                    id,
                    name,
                    gender,
                    phone_number AS PhoneNumber,
                    birthdate AS BirthDate,
                    register_date AS RegisterDate,
                    status AS Status,
                    memo AS Memo
                FROM tb_member
                WHERE id = @MemberId;";

            var cmd = new CommandDefinition(sql, new { MemberId = memberId }, transaction: tx, cancellationToken: ct);
            return await conn.QuerySingleOrDefaultAsync<Member>(cmd);
        }

        private static string ToCheckinDate(DateTime date)
        {
            return date.Date.ToString("yyyy-MM-dd");
        }

        private static CheckInResult Fail(
            CheckInResultType resultType,
            int memberId,
            string memberName,
            DateTime checkedInAt,
            string message)
        {
            return new CheckInResult
            {
                Success = false,
                ResultType = resultType,
                MemberId = memberId,
                MemberName = memberName,
                CheckedInAt = checkedInAt,
                Message = message
            };
        }

        private static MemberMembership? SelectGymMembership(
            IReadOnlyList<MemberMembership> memberships,
            DateTime attendanceDate)
        {
            return memberships
                .Where(x => x.Category == ProductCategory.Gym)
                .Where(x => IsActiveUsable(x, attendanceDate) || IsFirstCheckInPending(x))
                .OrderBy(x => IsActiveUsable(x, attendanceDate) ? 0 : 1)
                .ThenBy(x => x.EndDate ?? DateTime.MaxValue)
                .ThenBy(x => x.PurchasedAt)
                .ThenBy(x => x.Id)
                .FirstOrDefault();
        }

        private static bool IsActiveUsable(MemberMembership membership, DateTime attendanceDate)
        {
            if (membership.Status is MembershipStatus.Cancelled or MembershipStatus.Paused or MembershipStatus.Completed or MembershipStatus.Expired)
                return false;

            if (!membership.StartDate.HasValue || membership.StartDate.Value.Date > attendanceDate.Date)
                return false;

            if (membership.EndDate.HasValue && attendanceDate.Date > membership.EndDate.Value.Date)
                return false;

            if (membership.UsageType == ProductUsageType.Count && (membership.RemainingCount ?? 0) <= 0)
                return false;

            return membership.Status == MembershipStatus.Active;
        }

        private static bool IsFirstCheckInPending(MemberMembership membership)
        {
            return membership.StartType == ProductStartType.FirstCheckIn
                && membership.Status == MembershipStatus.Pending
                && !membership.StartDate.HasValue
                && !membership.ActivatedAt.HasValue;
        }

        private static void ActivateFirstCheckInMembership(MemberMembership membership, DateTime checkedInAt, DateTime now)
        {
            membership.StartDate = checkedInAt.Date;
            membership.ActivatedAt = checkedInAt;
            membership.Status = MembershipStatus.Active;
            membership.UpdatedAt = now;

            if (membership.UsageType == ProductUsageType.Period)
                membership.EndDate = MembershipDatePolicy.CalculatePeriodEndDate(checkedInAt, membership.UsageValue);
        }

        private static void DecrementCountMembership(MemberMembership membership, DateTime now)
        {
            var remaining = membership.RemainingCount ?? membership.UsageValue;
            if (remaining <= 0)
                throw new InvalidOperationException("이용권 잔여 횟수가 부족합니다.");

            membership.UsedCount = (membership.UsedCount ?? 0) + 1;
            membership.RemainingCount = remaining - 1;
            membership.UpdatedAt = now;

            if (membership.RemainingCount == 0)
                membership.Status = MembershipStatus.Completed;
        }

        private static MemberMembership Snapshot(MemberMembership membership)
        {
            return new MemberMembership
            {
                Id = membership.Id,
                ProductNameSnapshot = membership.ProductNameSnapshot,
                Category = membership.Category,
                UsageType = membership.UsageType,
                Status = membership.Status,
                UsedCount = membership.UsedCount,
                RemainingCount = membership.RemainingCount
            };
        }

        private static MembershipUsageResult CreateUsageResult(
            MemberMembership before,
            MemberMembership after,
            bool activated,
            bool decremented)
        {
            return new MembershipUsageResult
            {
                MembershipId = after.Id,
                ProductName = after.ProductNameSnapshot,
                Category = after.Category,
                UsageType = after.UsageType,
                BeforeStatus = before.Status,
                AfterStatus = after.Status,
                BeforeRemainingCount = before.RemainingCount,
                AfterRemainingCount = after.RemainingCount,
                ActivatedByFirstCheckIn = activated,
                Decremented = decremented,
                Message = CreateUsageMessage(after, activated, decremented)
            };
        }

        private static string CreateUsageMessage(MemberMembership membership, bool activated, bool decremented)
        {
            if (activated && decremented)
                return "첫 출석으로 활성화하고 1회 차감했습니다.";

            if (activated)
                return "첫 출석으로 활성화했습니다.";

            if (decremented)
                return "1회 차감했습니다.";

            return membership.UsageType == ProductUsageType.Period
                ? "기간 이용권으로 출석했습니다."
                : "출석 처리되었습니다.";
        }
    }
}
