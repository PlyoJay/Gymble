using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum ProductUsageType
{
    [Description("전체")]
    All,
    [Description("기간제")]
    Period,
    [Description("횟수제")]
    Count
}

public enum ProductStartType
{
    [Description("결제 즉시")]
    Immediate,
    [Description("직접 선택")]
    SelectDate,
    [Description("첫 출석 시작")]
    FirstCheckIn,
    [Description("고정 날짜")]
    FixedDate
}

public enum ProductStatus
{
    [Description("판매중")]
    OnSale,        // 판매중
    [Description("판매중지")]
    Stopped,       // 판매중지
    [Description("단종")]
    Discontinued   // 단종
}

public enum ProductCategory 
{
    [Description("헬스권")]
    Gym,
    [Description("PT")]
    PT,
    [Description("락커")]
    Locker,
    [Description("운동복")]
    Wear,
    [Description("기타")]
    Etc 
}

public enum TrainerAssignPolicy
{
    None,           // 트레이너 개념 없음
    FixedTrainer,   // 상품에 트레이너 고정
    SelectOnSale    // 판매/등록 시 트레이너 선택
}

public enum LockerAssignPolicy
{
    SelectOnSale,      // 등록할 때 락커 번호 선택
    AutoAssignFromPool // 빈 락커 자동 배정(나중 확장)
}


namespace Gymble.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public ProductCategory Category { get; set; }
        public int Price { get; set; }
        public ProductUsageType UsageType { get; set; }
        //public int? DurationDays { get; set; } // Period
        //public int? TotalCount { get; set; }   // Count
        public int? UsageValue { get; set; } // 사용자가 입력하는 값 (기간/횟수)
        public ProductStartType StartType { get; set; }
        public DateTime? FixedStartDate { get; set; }
        public ProductStatus Status { get; set; }
        public bool IsFavorite { get; set; } // 자주 쓰는 상품
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ProductPtOption
    {
        public int ProductId { get; set; }

        public TrainerAssignPolicy AssignPolicy { get; set; }
        public int? FixedTrainerId { get; set; } // FixedTrainer일 때
    }

    public class ProductLockerOption
    {
        public int ProductId { get; set; }

        public LockerAssignPolicy AssignPolicy { get; set; }
        public int? LockerGroupId { get; set; } // 남/여, 1층/2층 등 그룹
    }

    public sealed class ProductSearch
    {
        public string? NameOrCode { get; set; }
        public ProductCategory SelectedCategory { get; set; }
        public List<ProductStatus>? Statuses { get; set; }
        public ProductUsageType UsageType { get; set; }
        public int? MinUsageValue { get; set; }
        public int? MaxUsageValue { get; set; }

        public int? MinPrice { get; set; }
        public int? MaxPrice { get; set; }
        public ProductStartType? StartType { get; set; }
        public string SortBy { get; set; } = "register_date"; // name, register_date, phone_number
        public bool Desc { get; set; } = true;

        public int? Take { get; set; }
        public int? Skip { get; set; }
    }
}
