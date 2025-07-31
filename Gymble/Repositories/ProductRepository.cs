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
    public class ProductRepository
    {
        private readonly SQLiteConnection _connection;

        public ProductRepository(SQLiteConnection connection)
        {
            _connection = connection;
        }

        public void InsertMember(Product product)
        {
            var sql = @"INSERT INTO tb_product (name, type, duration_days, total_count, price)
                    VALUES (@Name, @Type, @DurationDays, @TotalCount, @Price)";
            _connection.Execute(sql, product);
        }

        public List<Product> GetAllMembers()
        {
            var sql = "SELECT * FROM tb_product";
            return _connection.Query<Product>(sql).ToList();
        }
    }
}
