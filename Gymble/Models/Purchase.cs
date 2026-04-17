using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum PaymentMethod
{
    [Description("현금")]
    Cash = 0,

    [Description("카드")]
    Card = 1,

    [Description("계좌이체")]
    BankTransfer = 2
}

public enum PurchaseStatus
{
    [Description("결제완료")]
    Completed = 0,

    [Description("취소")]
    Cancelled = 1
}


namespace Gymble.Models
{
    public class Purchase
    {
        public int Id { get; set; }
        public int MemberId { get; set; }

        public int TotalAmount { get; set; }
        public int DiscountAmount { get; set; }
        public int FinalAmount { get; set; }

        public PaymentMethod PaymentMethod { get; set; }
        public PurchaseStatus Status { get; set; }

        public DateTime PurchasedAt { get; set; }
        public string? Memo { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class PurchaseItem
    {
        public int Id { get; set; }
        public int PurchaseId { get; set; }

        public int ProductId { get; set; }
        public string ProductNameSnapshot { get; set; } = "";
        public string ProductCodeSnapshot { get; set; } = "";

        public ProductCategory Category { get; set; }
        public ProductUsageType? UsageType { get; set; }
        public ProductStartType? StartType { get; set; }

        public int UnitPrice { get; set; }
        public int LineAmount { get; set; }

        public int? UsageValue { get; set; }

        public bool IsMembershipItem { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class PurchaseRequest
    {
        public int MemberId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public int DiscountAmount { get; set; }
        public string? Memo { get; set; }

        public List<PurchaseRequestItem> Items { get; set; } = new();
    }

    public class PurchaseRequestItem
    {
        public int ProductId { get; set; }
        public DateTime? SelectedStartDate { get; set; } // 지정 시작일 필요 시
    }
}
