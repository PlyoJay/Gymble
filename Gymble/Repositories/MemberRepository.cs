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
            var sql = @"INSERT INTO tb_member (name, gender, phonenumber, birthdate, registerdate, memo)
                    VALUES (@Name, @Gender, @PhoneNumber, @BirthDate, @RegisterDate, @Memo)";
            _connection.Execute(sql, member);
        }

        public int InsertMemberFillingHoles(Member member)
        {
            using var tx = _connection.BeginTransaction();

            // 1) 최소 누락 ID 확보
            const string sqlNextId = @"
                SELECT CASE
                  WHEN NOT EXISTS (SELECT 1 FROM tb_member WHERE id = 1) THEN 1
                  ELSE (
                    SELECT m1.id + 1
                    FROM tb_member AS m1
                    LEFT JOIN tb_member AS m2 ON m2.id = m1.id + 1
                    WHERE m2.id IS NULL
                    ORDER BY m1.id
                    LIMIT 1
                  )
                END AS next_id;";

            int nextId = _connection.ExecuteScalar<int>(sqlNextId, transaction: tx);

            // 2) 명시적 ID로 INSERT
            const string sqlInsert = @"
                INSERT INTO tb_member (id, name, gender, phone_number, birthdate, register_date, memo)
                VALUES (@Id, @Name, @Gender, @PhoneNumber, @BirthDate, @RegisterDate, @Memo);";

            // 컬럼명은 실제 스키마에 맞추세요 (phone_number / register_date 등)
            _connection.Execute(sqlInsert, new
            {
                Id = nextId,
                member.Name,
                member.Gender,
                member.PhoneNumber,
                member.BirthDate,
                member.RegisterDate,
                member.Memo
            }, tx);

            tx.Commit();
            return nextId;
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
                "SET name = @Name, gender = @Gender, phonenumber = @PhoneNumber, " +
                "birthdate = @BirthDate, registerdate = @RegisterDate, memo = @Memo" +
                "WHERE id = @Id";
            _connection.Execute(sql, member);
        }

        public void SearchMember()
        {

        }
    }
}
