using Dapper;
using Gymble.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gymble.Repositories
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly SQLiteConnection _conn;

        public AttendanceRepository(SQLiteConnection connection)
            => _conn = connection ?? throw new ArgumentNullException(nameof(connection));

        public async Task<bool> HasCheckedInAsync(long memberId, string checkinDate, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT EXISTS(
                    SELECT 1 FROM tb_attendance
                    WHERE member_id = @MemberId AND checkin_date = @Date
                );";
            var cmd = new CommandDefinition(sql, new { MemberId = memberId, Date = checkinDate }, cancellationToken: ct);
            return await _conn.ExecuteScalarAsync<long>(cmd) == 1;
        }

        public async Task<long> CheckInOncePerDayAsync(long memberId, DateTime checkedInAtUtc, CancellationToken ct = default)
        {
            // 날짜키 생성 (UTC 기준). 표시만 KST로 하면 됨.
            string dateKey = checkedInAtUtc.ToString("yyyy-MM-dd");

            const string sql = @"
                INSERT INTO tb_attendance (member_id, datetime, checkin_date)
                VALUES (@MemberId, @CheckedInAt, @DateKey);
                SELECT last_insert_rowid();";

            try
            {
                var cmd = new CommandDefinition(sql, new
                {
                    MemberId = memberId,
                    CheckedInAt = checkedInAtUtc,
                    DateKey = dateKey
                }, cancellationToken: ct);

                return await _conn.ExecuteScalarAsync<long>(cmd);
            }
            catch (SQLiteException ex) when (ex.ResultCode == SQLiteErrorCode.Constraint)
            {
                // ux_attendance_member_day에 걸림 = 오늘 이미 체크인 함
                return -1;
            }
        }

        public async Task<IReadOnlyList<Attendance>> GetByDateAsync(string checkinDate, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT id,
                       member_id AS MemberId,
                       datetime   AS CheckedInAt,
                       checkin_date AS CheckinDate
                FROM tb_attendance
                WHERE checkin_date = @Date
                ORDER BY datetime DESC;";

            var cmd = new CommandDefinition(sql, new { Date = checkinDate }, cancellationToken: ct);
            var rows = await _conn.QueryAsync<Attendance>(cmd);
            return rows.AsList();
        }
    }
}
