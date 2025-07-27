using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gymble.Models
{
    public class Attendance
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public DateTime DateTime { get; set; }
    }
}
