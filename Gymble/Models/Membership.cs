using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum MembershipStatus
{
    Pending = 0,      // 구매됨, 아직 시작 안 함
    Active = 1,       // 사용 중
    Paused = 2,       // 일시정지
    Expired = 3,      // 만료
    Completed = 4,    // 횟수 모두 소진
    Cancelled = 5     // 취소
}

namespace Gymble.Models
{    
    public class MemberMembership
    {
        public int Id { get; set; }

        public int MemberId { get; set; }
        public int PurchaseId { get; set; }
        public int PurchaseItemId { get; set; }

        public int ProductId { get; set; }
        public string ProductCodeSnapshot { get; set; } = "";
        public string ProductNameSnapshot { get; set; } = "";

        public ProductCategory Category { get; set; }
        public ProductUsageType UsageType { get; set; }
        public ProductStartType StartType { get; set; }

        public int UnitPriceSnapshot { get; set; }
        public int UsageValue { get; set; }

        public int? DurationDays { get; set; }

        public int? TotalCount { get; set; }
        public int? UsedCount { get; set; }
        public int? RemainingCount { get; set; }

        public DateTime PurchasedAt { get; set; }
        public DateTime? ActivatedAt { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public MembershipStatus Status { get; set; }
        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
