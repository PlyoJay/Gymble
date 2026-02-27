using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum MemberStatus
{
    Active = 0,     // 사용중
    Paused = 1,    // 일시정지
    Suspended = 2,  // 정지
    Expired = 3   // 만료
}

namespace Gymble.Models
{
    public class Member
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Gender { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime RegisterDate { get; set; }
        public MemberStatus Status { get; set; }
        public string? Memo { get; set; }
    }

    public sealed class MemberSearch
    {
        public string? NameOrPhone { get; set; }
        public DateTime? RegFrom { get; set; }
        public DateTime? RegTo { get; set; }
        public string SortBy { get; set; } = "register_date"; // name, register_date, phone_number
        public bool Desc { get; set; } = true;
        public int Page { get; set; } = 1;        // 1-base
        public int PageSize { get; set; } = 50;   // 50/100/200
    }

    public sealed class PagedResult<T>
    {
        public IReadOnlyList<T> Rows { get; init; }
        public int Total { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalPages => (int)Math.Ceiling((double)Total / Math.Max(1, PageSize));
    }
}
