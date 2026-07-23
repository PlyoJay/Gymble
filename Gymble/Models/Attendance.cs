using System.Data.SQLite;

namespace Gymble.Models
{
    public class Attendance
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public DateTime CheckedInAt { get; set; }
        public string CheckinDate { get; set; } = "";
        public int? MembershipId { get; set; }
        public int? MembershipBeforeRemaining { get; set; }
        public int? MembershipAfterRemaining { get; set; }
    }

    public enum CheckInResultType
    {
        Success,
        AlreadyCheckedIn,
        MemberNotFound,
        MemberUnavailable,
        NoUsableMembership,
        Failed
    }

    public sealed class MembershipUsageResult
    {
        public int MembershipId { get; init; }
        public string ProductName { get; init; } = "";
        public ProductCategory Category { get; init; }
        public ProductUsageType UsageType { get; init; }
        public MembershipStatus BeforeStatus { get; init; }
        public MembershipStatus AfterStatus { get; init; }
        public int? BeforeRemainingCount { get; init; }
        public int? AfterRemainingCount { get; init; }
        public bool ActivatedByFirstCheckIn { get; init; }
        public bool Decremented { get; init; }
        public string Message { get; init; } = "";
    }

    public sealed class CheckInResult
    {
        public bool Success { get; init; }
        public CheckInResultType ResultType { get; init; }
        public long? AttendanceId { get; init; }
        public int MemberId { get; init; }
        public string MemberName { get; init; } = "";
        public DateTime CheckedInAt { get; init; }
        public IReadOnlyList<MembershipUsageResult> MembershipResults { get; init; } = Array.Empty<MembershipUsageResult>();
        public string Message { get; init; } = "";
    }

    public sealed class AttendanceViewItem
    {
        public long AttendanceId { get; init; }
        public int MemberId { get; init; }
        public string MemberName { get; init; } = "";
        public string? PhoneNumber { get; init; }
        public DateTime CheckedInAt { get; init; }
        public string CheckInTimeText => CheckedInAt.ToString("HH:mm");
        public int? MembershipId { get; init; }
        public string? MembershipName { get; init; }
        public ProductUsageType? UsageType { get; init; }
        public string UsageTypeText => UsageType switch
        {
            ProductUsageType.Period => "기간권",
            ProductUsageType.Count => "횟수권",
            _ => ""
        };
        public int? BeforeRemainingCount { get; init; }
        public int? AfterRemainingCount { get; init; }
        public string UsageChangeText
        {
            get
            {
                if (!BeforeRemainingCount.HasValue || !AfterRemainingCount.HasValue)
                    return UsageType == ProductUsageType.Period ? "기간 이용권" : "";

                return $"{BeforeRemainingCount:N0}회 -> {AfterRemainingCount:N0}회";
            }
        }
        public string ResultText { get; init; } = "";
    }

    public interface IAttendanceRepository
    {
        Task<bool> HasCheckedInAsync(long memberId, string checkinDate, CancellationToken ct = default);
        Task<IReadOnlyList<AttendanceViewItem>> GetByDateAsync(string checkinDate, CancellationToken ct = default);

        Task<bool> HasCheckedInAsync(
            SQLiteConnection conn,
            SQLiteTransaction tx,
            long memberId,
            string checkinDate,
            CancellationToken ct = default);

        Task<long> InsertAsync(
            SQLiteConnection conn,
            SQLiteTransaction tx,
            Attendance attendance,
            CancellationToken ct = default);
    }
}
