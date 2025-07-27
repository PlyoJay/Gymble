using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gymble.Models
{
    public class Membership
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public int ProductId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpireDate { get; set; } // 기간제의 종료일
        public int RemainingCount { get; set; } //횟수제의 남은 횟수
    }
}
