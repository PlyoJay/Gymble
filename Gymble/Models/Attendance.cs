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
        // 기존 DateTime -> CheckedInAt 추천
        public DateTime CheckedInAt { get; set; }

        // 하루 1회 보장을 위한 날짜키 (YYYY-MM-DD)
        public string CheckinDate { get; set; } = "";
    }

    public interface IAttendanceRepository
    {
        Task<long> CheckInOncePerDayAsync(long memberId, DateTime checkedInAtUtc, CancellationToken ct = default);
        Task<bool> HasCheckedInAsync(long memberId, string checkinDate, CancellationToken ct = default);
        Task<IReadOnlyList<Attendance>> GetByDateAsync(string checkinDate, CancellationToken ct = default);
    }
}
