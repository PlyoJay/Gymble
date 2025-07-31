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

        public void InsertAttendace(Attendance attendace)
        {
            var sql = @"INSERT INTO tb_attendace (member_id, datetime)
                    VALUES (@MemberId, @DateTime)";
            _connection.Execute(sql, attendace);
        }

        public List<Attendance> GetAllAttendace()
        {
            var sql = "SELECT * FROM tb_attendace";
            return _connection.Query<Attendance>(sql).ToList();
        }
    }
}
