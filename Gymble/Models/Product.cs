using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gymble.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; } // 기간제 or 횟수제
        public int DurationDays { get; set; } // 기간제일 경우
        public int TotalCount { get; set; } // 횟수제일 경우
        public int Price { get; set; }
    }
}
