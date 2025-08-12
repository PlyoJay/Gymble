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
    public class AttendanceRepository
    {
        private readonly SQLiteConnection _connection;

        public AttendanceRepository(SQLiteConnection connection)
        {
            _connection = connection;
        }

        public void InsertAttendace(Attendance attendance)
        {
            var sql = @"INSERT INTO tb_attendance (member_id, datetime)
                    VALUES (@MemberId, @DateTime)";
            _connection.Execute(sql, attendance);
        }

        public List<Attendance> GetAllAttendace()
        {
            var sql = "SELECT * FROM tb_attendance";
            return _connection.Query<Attendance>(sql).ToList();
        }
    }
}
