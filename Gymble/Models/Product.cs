using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum ProductUsageType
{
    Period,     // 기간제
    Count       // 횟수제
}

public enum ProductStartType
{
    Immediate,        // 결제 즉시 시작
    SelectDate,       // 사용자가 시작일 선택
    FirstCheckIn,     // 첫 출석 시 시작
    FixedDate         // 고정 날짜
}

public enum ProductStatus
{
    OnSale,        // 판매중
    Stopped,       // 판매중지
    Discontinued   // 단종
}

public enum ProductCategory 
{ 
    Gym, 
    PT, 
    Locker, 
    Wear, 
    Etc 
}

namespace Gymble.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public ProductCategory Category { get; set; }
        public int Price { get; set; }
        public ProductUsageType UsageType { get; set; }
        public int? DurationDays { get; set; } // Period
        public int? TotalCount { get; set; }   // Count

        public ProductStartType StartType { get; set; }
        public ProductStatus Status { get; set; }

        public bool IsFavorite { get; set; } // 자주 쓰는 상품

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
