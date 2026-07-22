using Gymble.Models;
using Gymble.Repositories;

namespace Gymble.Services
{
    public interface IMembershipService
    {
        Task<IReadOnlyList<MemberMembershipSummary>> GetByMemberIdAsync(long memberId, CancellationToken ct = default);
        Task ActivateFirstCheckInMembershipsAsync(long memberId, DateTime checkedInAt, CancellationToken ct = default);
    }

    public sealed class MembershipService : IMembershipService
    {
        private readonly IMembershipRepository _membershipRepository;

        public MembershipService(IMembershipRepository membershipRepository)
            => _membershipRepository = membershipRepository ?? throw new ArgumentNullException(nameof(membershipRepository));

        public async Task<IReadOnlyList<MemberMembershipSummary>> GetByMemberIdAsync(long memberId, CancellationToken ct = default)
        {
            if (memberId <= 0)
                return Array.Empty<MemberMembershipSummary>();

            var memberships = await _membershipRepository.GetByMemberIdAsync(memberId, ct);
            var today = DateTime.Today;

            return memberships
                .Select(x => CreateSummary(x, today))
                .OrderByDescending(x => x.Status == MembershipStatus.Active)
                .ThenByDescending(x => x.PurchasedAt)
                .ToList();
        }

        public async Task ActivateFirstCheckInMembershipsAsync(long memberId, DateTime checkedInAt, CancellationToken ct = default)
        {
            if (memberId <= 0)
                throw new ArgumentException("회원 ID가 올바르지 않습니다.", nameof(memberId));

            var memberships = await _membershipRepository.GetByMemberIdAsync(memberId, ct);
            var now = DateTime.Now;

            foreach (var membership in memberships.Where(CanActivateOnFirstCheckIn))
            {
                membership.StartDate = checkedInAt.Date;
                membership.ActivatedAt = checkedInAt;
                membership.Status = MembershipStatus.Active;
                membership.UpdatedAt = now;

                if (membership.UsageType == ProductUsageType.Period)
                    membership.EndDate = MembershipDatePolicy.CalculatePeriodEndDate(checkedInAt, membership.UsageValue);

                await _membershipRepository.UpdateActivationAsync(membership, ct);
            }
        }

        private static bool CanActivateOnFirstCheckIn(MemberMembership membership)
        {
            return membership.StartType == ProductStartType.FirstCheckIn
                && membership.Status == MembershipStatus.Pending
                && !membership.StartDate.HasValue
                && !membership.ActivatedAt.HasValue;
        }

        private static MemberMembershipSummary CreateSummary(MemberMembership membership, DateTime today)
        {
            var status = CalculateStatus(membership, today);

            return new MemberMembershipSummary
            {
                Id = membership.Id,
                MemberId = membership.MemberId,
                ProductId = membership.ProductId,
                ProductName = membership.ProductNameSnapshot,
                Category = membership.Category,
                UsageType = membership.UsageType,
                StartType = membership.StartType,
                UsageValue = membership.UsageValue,
                PurchasedAt = membership.PurchasedAt,
                ActivatedAt = membership.ActivatedAt,
                StartDate = membership.StartDate,
                EndDate = membership.EndDate,
                TotalCount = membership.TotalCount,
                UsedCount = membership.UsedCount,
                RemainingCount = membership.RemainingCount,
                Status = status,
                StatusText = ToStatusText(status),
                PeriodText = ToPeriodText(membership),
                UsageText = ToUsageText(membership)
            };
        }

        private static MembershipStatus CalculateStatus(MemberMembership membership, DateTime today)
        {
            if (membership.Status is MembershipStatus.Cancelled or MembershipStatus.Paused)
                return membership.Status;

            if (membership.UsageType == ProductUsageType.Count && membership.RemainingCount <= 0)
                return MembershipStatus.Completed;

            if (!membership.StartDate.HasValue)
                return MembershipStatus.Pending;

            if (membership.StartDate.Value.Date > today.Date)
                return MembershipStatus.Pending;

            if (membership.EndDate.HasValue && today.Date > membership.EndDate.Value.Date)
                return MembershipStatus.Expired;

            return MembershipStatus.Active;
        }

        private static string ToStatusText(MembershipStatus status)
        {
            return status switch
            {
                MembershipStatus.Pending => "대기",
                MembershipStatus.Active => "이용중",
                MembershipStatus.Paused => "일시정지",
                MembershipStatus.Expired => "만료",
                MembershipStatus.Completed => "소진",
                MembershipStatus.Cancelled => "취소",
                _ => status.ToString()
            };
        }

        private static string ToPeriodText(MemberMembership membership)
        {
            if (!membership.StartDate.HasValue)
                return "시작 전";

            if (!membership.EndDate.HasValue)
                return $"{membership.StartDate:yyyy-MM-dd} ~";

            return $"{membership.StartDate:yyyy-MM-dd} ~ {membership.EndDate:yyyy-MM-dd}";
        }

        private static string ToUsageText(MemberMembership membership)
        {
            return membership.UsageType switch
            {
                ProductUsageType.Period => $"{membership.UsageValue:N0}일",
                ProductUsageType.Count => $"{membership.RemainingCount ?? 0:N0}/{membership.TotalCount ?? membership.UsageValue:N0}회",
                _ => ""
            };
        }
    }
}
