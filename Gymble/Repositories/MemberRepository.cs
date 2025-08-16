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
    public class MemberRepository
    {
        private readonly SQLiteConnection _connection;

        public MemberRepository(SQLiteConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public void InsertMember(Member member)
        {
            var sql = @"INSERT INTO tb_member (name, gender, phone_number, birthdate, register_date, memo)
                    VALUES (@Name, @Gender, @PhoneNumber, @BirthDate, @RegisterDate, @Memo)";
            _connection.Execute(sql, member);
        }

        public List<Member> GetAllMembers()
        {
            var sql = "SELECT * FROM tb_member";
            return _connection.Query<Member>(sql).ToList();
        }

        public void DeleteMember(Member member)
        {
            var sql = "DELETE FROM tb_member WHERE id = @Id";
            _connection.Execute(sql, member);
        }

        public void UpdateMember(Member member)
        {
            var sql = 
                "UPDATE tb_member" +
                "SET name = @Name, gender = @Gender, phone_number = @PhoneNumber, " +
                "birthdate = @BirthDate, register_date = @RegisterDate, memo = @Memo" +
                "WHERE id = @Id";
            _connection.Execute(sql, member);
        }

        public void SearchMember()
        {

        }
    }
}
