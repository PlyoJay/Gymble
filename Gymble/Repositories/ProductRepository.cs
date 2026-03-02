using Dapper;
using Gymble.Models;
using Gymble.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Gymble.Repositories
{
    public interface IProductRepository
    {
        Task<IReadOnlyList<Product>> SearchAsync(ProductSearch q, CancellationToken ct = default);
        Task<Product?> GetByIdAsync(long productId, CancellationToken ct = default);
        Task<long> InsertProductAsync(Product prodcut, CancellationToken ct = default);
        Task<int> UpdateProductAsync(Product product, CancellationToken ct = default);
        Task<int> DeleteProductAsync(long productId, CancellationToken ct = default);
        Task<string> GenerateAsync(ProductCategory category, CancellationToken ct = default);
    }

    public class ProductRepository : IProductRepository
    {
        private readonly Func<SQLiteConnection> _connFactory;

        public ProductRepository(Func<SQLiteConnection> connFactory)
            => _connFactory = connFactory;

        public async Task<IReadOnlyList<Product>> SearchAsync(ProductSearch q, CancellationToken ct = default)
        {
            using var conn = _connFactory();
            q ??= new ProductSearch();

            // 범위 정규화
            if (q.MinPrice.HasValue && q.MaxPrice.HasValue && q.MinPrice > q.MaxPrice)
                (q.MinPrice, q.MaxPrice) = (q.MaxPrice, q.MinPrice);

            if (q.MinUsageValue.HasValue && q.MaxUsageValue.HasValue && q.MinUsageValue > q.MaxUsageValue)
                (q.MinUsageValue, q.MaxUsageValue) = (q.MaxUsageValue, q.MinUsageValue);

            // SortBy 화이트리스트
            string orderBy = q.SortBy switch
            {
                "name" => "name",
                "code" => "code",
                "price" => "price",
                "created_at" => "created_at",
                _ => "created_at"
            };
            string dir = q.Desc ? "DESC" : "ASC";

            var where = new List<string>();
            var p = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(q.NameOrCode))
            {
                where.Add("(name LIKE @NameOrCode OR code LIKE @NameOrCode)");
                p.Add("NameOrCode", $"%{q.NameOrCode}%");
            }

            if (q.Statuses is { Count: > 0 })
            {
                where.Add("status IN (@Statuses)"); // <- SQLite에서 안전
                p.Add("Statuses", q.Statuses);
            }

            if (q.UsageType.HasValue)
            {
                where.Add("usage_type = @UsageType");
                p.Add("UsageType", q.UsageType);
            }

            if (q.MinUsageValue.HasValue)
            {
                where.Add("usage_value >= @MinUsageValue");
                p.Add("MinUsageValue", q.MinUsageValue);
            }

            if (q.MaxUsageValue.HasValue)
            {
                where.Add("usage_value <= @MaxUsageValue");
                p.Add("MaxUsageValue", q.MaxUsageValue);
            }

            if (q.MinPrice.HasValue)
            {
                where.Add("price >= @MinPrice");
                p.Add("MinPrice", q.MinPrice);
            }

            if (q.MaxPrice.HasValue)
            {
                where.Add("price <= @MaxPrice");
                p.Add("MaxPrice", q.MaxPrice);
            }

            if (q.StartType.HasValue)
            {
                where.Add("start_type = @StartType");
                p.Add("StartType", q.StartType);
            }

            string whereSql = where.Count == 0 ? "" : "WHERE " + string.Join(" AND ", where);

            string sql = $@"
                SELECT *
                FROM tb_product
                {whereSql}
                ORDER BY {orderBy} {dir};";

            var cmd = new CommandDefinition(sql, p, cancellationToken: ct);
            return (await conn.QueryAsync<Product>(cmd)).AsList();
        }

        public Task<Product?> GetByIdAsync(long productId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<long> InsertProductAsync(Product product, CancellationToken ct = default)
        {
            using var conn = _connFactory();
            if (conn.State != ConnectionState.Open) conn.Open();

            const string sql = SqlProductQuery.INSERT_PRODUCT;

            var cmd = new CommandDefinition(sql, product, cancellationToken: ct);
            return await conn.ExecuteScalarAsync<long>(cmd);
        }


        public Task<int> UpdateProductAsync(Product product, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeleteProductAsync(long productId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GenerateAsync(ProductCategory category, CancellationToken ct = default)
        {
            string prefix = category switch
            {
                ProductCategory.Gym => "GYM",
                ProductCategory.PT => "PT",
                ProductCategory.Locker => "LOCK",
                ProductCategory.Wear => "WEAR",
                _ => "ETC"
            };

            using var conn = _connFactory();
            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();

            using var tx = conn.BeginTransaction();

            const string insertIfMissing = @"
                INSERT OR IGNORE INTO tb_code_sequence(prefix, last_value)
                VALUES (@Prefix, 0);
            ";

            const string update = @"
                UPDATE tb_code_sequence
                SET last_value = last_value + 1
                WHERE prefix = @Prefix;
            ";

            const string select = @"
                SELECT last_value
                FROM tb_code_sequence
                WHERE prefix = @Prefix;
            ";

            var param = new { Prefix = prefix };

            await conn.ExecuteAsync(new CommandDefinition(insertIfMissing, param, transaction: tx, cancellationToken: ct));
            await conn.ExecuteAsync(new CommandDefinition(update, param, transaction: tx, cancellationToken: ct));

            int seq = await conn.ExecuteScalarAsync<int>(new CommandDefinition(select, param, transaction: tx, cancellationToken: ct));

            tx.Commit();

            return $"{prefix}-{seq:0000}";
        }
    }
}
