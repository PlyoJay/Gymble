namespace Gymble.Models
{
    public sealed class MemberMembershipSummary
    {
        public int Id { get; init; }
        public int MemberId { get; init; }
        public int ProductId { get; init; }
        public string ProductName { get; init; } = "";
        public ProductCategory Category { get; init; }
        public ProductUsageType UsageType { get; init; }
        public ProductStartType StartType { get; init; }
        public int UsageValue { get; init; }
        public DateTime PurchasedAt { get; init; }
        public DateTime? ActivatedAt { get; init; }
        public DateTime? StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public int? TotalCount { get; init; }
        public int? UsedCount { get; init; }
        public int? RemainingCount { get; init; }
        public MembershipStatus Status { get; init; }
        public string StatusText { get; init; } = "";
        public string PeriodText { get; init; } = "";
        public string UsageText { get; init; } = "";
    }
}
