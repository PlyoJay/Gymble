using Dapper;
using Gymble.Models;
using Gymble.Services;
using System.Data.SQLite;

namespace Gymble.Repositories
{
    public sealed class AttendanceRepository : IAttendanceRepository
    {
        private readonly Func<SQLiteConnection> _connFactory;

        public AttendanceRepository(Func<SQLiteConnection> connFactory)
            => _connFactory = connFactory ?? throw new ArgumentNullException(nameof(connFactory));

        public async Task<bool> HasCheckedInAsync(long memberId, string checkinDate, CancellationToken ct = default)
        {
            using var conn = _connFactory();
            var cmd = new CommandDefinition(
                SqlAttendanceQuery.HAS_CHECKED_IN,
                new { MemberId = memberId, CheckinDate = checkinDate },
                cancellationToken: ct);

            return await conn.ExecuteScalarAsync<long>(cmd) == 1;
        }

        public async Task<bool> HasCheckedInAsync(
            SQLiteConnection conn,
            SQLiteTransaction tx,
            long memberId,
            string checkinDate,
            CancellationToken ct = default)
        {
            var cmd = new CommandDefinition(
                SqlAttendanceQuery.HAS_CHECKED_IN,
                new { MemberId = memberId, CheckinDate = checkinDate },
                transaction: tx,
                cancellationToken: ct);

            return await conn.ExecuteScalarAsync<long>(cmd) == 1;
        }

        public async Task<long> InsertAsync(
            SQLiteConnection conn,
            SQLiteTransaction tx,
            Attendance attendance,
            CancellationToken ct = default)
        {
            var cmd = new CommandDefinition(
                SqlAttendanceQuery.INSERT_ATTENDANCE,
                attendance,
                transaction: tx,
                cancellationToken: ct);

            return await conn.ExecuteScalarAsync<long>(cmd);
        }

        public async Task<IReadOnlyList<AttendanceViewItem>> GetByDateAsync(string checkinDate, CancellationToken ct = default)
        {
            using var conn = _connFactory();
            var cmd = new CommandDefinition(
                SqlAttendanceQuery.GET_BY_DATE,
                new { CheckinDate = checkinDate },
                cancellationToken: ct);

            return (await conn.QueryAsync<AttendanceViewItem>(cmd)).AsList();
        }
    }
}
