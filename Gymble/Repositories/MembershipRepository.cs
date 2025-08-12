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
    public class MembershipRepository
    {
        private readonly SQLiteConnection _connection;

        public MembershipRepository(SQLiteConnection connection)
        {
            _connection = connection;
        }

        public void InsertMembership(Membership membership)
        {
            var sql = @"INSERT INTO tb_membership (member_id, product_id, purchase_date, start_date, expire_date, remaining_count)
                    VALUES (@MemberId, @ProductId, @PurchaseDate, @StartDate, @ExpireDate, @RemainingCount)";
            _connection.Execute(sql, membership);
        }

        public List<Membership> GetAllMembership()
        {
            var sql = "SELECT * FROM tb_membership";
            return _connection.Query<Membership>(sql).ToList();
        }
    }
}
