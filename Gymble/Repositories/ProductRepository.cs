using Dapper;
using Gymble.Models;
using Gymble.Services;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

namespace Gymble.Repositories
{
    public interface IProductRepository
    {
        Task<IReadOnlyList<Product>> SearchAsync(ProductSearch q, CancellationToken ct = default);
        Task<Product?> GetByIdAsync(long productId, CancellationToken ct = default);

        Task<long> InsertProductAsync(Product product, CancellationToken ct = default);
        Task<int> UpdateProductAsync(Product product, CancellationToken ct = default);
        Task<int> DeleteProductAsync(long productId, CancellationToken ct = default);

        Task<long> InsertProductComponentAsync(ProductComponent component, CancellationToken ct = default);
        Task<int> DeleteProductComponentsAsync(long productId, CancellationToken ct = default);
        Task<IReadOnlyList<ProductComponent>> GetProductComponentsAsync(long productId, CancellationToken ct = default);

        Task<long> InsertProductWithComponentsAsync(Product product, IReadOnlyList<ProductComponent> components, CancellationToken ct = default);
        Task<int> UpdateProductWithComponentsAsync(Product product, IReadOnlyList<ProductComponent> components, CancellationToken ct = default);

        Task<string> GenerateAsync(ProductSaleType saleType, CancellationToken ct = default);

        Task<long> InsertProductAsync(SQLiteConnection conn, SQLiteTransaction tx, Product product, CancellationToken ct = default);
        Task<long> InsertProductComponentAsync(SQLiteConnection conn, SQLiteTransaction tx, ProductComponent component, CancellationToken ct = default);
        Task<int> UpdateProductAsync(SQLiteConnection conn, SQLiteTransaction tx, Product product, CancellationToken ct = default);
        Task<int> DeleteProductComponentsAsync(SQLiteConnection conn, SQLiteTransaction tx, long productId, CancellationToken ct = default);
    }

    public class ProductRepository : IProductRepository
    {
        private readonly Func<SQLiteConnection> _connFactory;

        public ProductRepository(Func<SQLiteConnection> connFactory)
            => _connFactory = connFactory;

        public async Task<IReadOnlyList<Product>> SearchAsync(ProductSearch q, CancellationToken ct = default)
        {
            using var conn = _connFactory();
            if (conn.State != ConnectionState.Open)
                conn.Open();

            q ??= new ProductSearch();

            string orderBy = q.SortBy switch
            {
                "name" => "p.name",
                "code" => "p.code",
                "price" => "p.price",
                "created_at" => "p.created_at",
                "updated_at" => "p.updated_at",
                _ => "p.created_at"
            };

            string dir = q.Desc ? "DESC" : "ASC";

            var where = new List<string>();
            var p = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(q.NameOrCode))
            {
                where.Add("(p.name LIKE @NameOrCode OR p.code LIKE @NameOrCode)");
                p.Add("NameOrCode", $"%{q.NameOrCode}%");
            }

            if (q.SaleType.HasValue)
            {
                where.Add("p.sale_type = @SaleType");
                p.Add("SaleType", q.SaleType.Value);
            }

            if (q.Statuses is { Count: > 0 })
            {
                where.Add("p.status IN @Statuses");
                p.Add("Statuses", q.Statuses);
            }

            if (q.MinPrice.HasValue)
            {
                where.Add("p.price >= @MinPrice");
                p.Add("MinPrice", q.MinPrice.Value);
            }

            if (q.MaxPrice.HasValue)
            {
                where.Add("p.price <= @MaxPrice");
                p.Add("MaxPrice", q.MaxPrice.Value);
            }

            if (q.IsFavorite.HasValue)
            {
                where.Add("p.is_favorite = @IsFavorite");
                p.Add("IsFavorite", q.IsFavorite.Value ? 1 : 0);
            }

            var componentWhere = new List<string>
            {
                "pc.product_id = p.id"
            };

            if (q.SelectedCategory.HasValue)
            {
                componentWhere.Add("pc.category = @ComponentCategory");
                p.Add("ComponentCategory", q.SelectedCategory.Value);
            }

            if (q.UsageType != ProductUsageType.All)
            {
                componentWhere.Add("pc.usage_type = @ComponentUsageType");
                p.Add("ComponentUsageType", q.UsageType);
            }

            if (q.MinUsageValue.HasValue)
            {
                componentWhere.Add("pc.usage_value >= @MinUsageValue");
                p.Add("MinUsageValue", q.MinUsageValue.Value);
            }

            if (q.MaxUsageValue.HasValue)
            {
                componentWhere.Add("pc.usage_value <= @MaxUsageValue");
                p.Add("MaxUsageValue", q.MaxUsageValue.Value);
            }

            if (q.StartType.HasValue)
            {
                componentWhere.Add("pc.start_type = @ComponentStartType");
                p.Add("ComponentStartType", q.StartType.Value);
            }

            if (componentWhere.Count > 1)
            {
                where.Add($@"
                    EXISTS (
                        SELECT 1
                        FROM tb_product_component pc
                        WHERE {string.Join(" AND ", componentWhere)}
                    )");
            }

            string whereSql = where.Count == 0
                ? ""
                : "WHERE " + string.Join(" AND ", where);

            string sql = $@"
                SELECT
                    id,
                    code,
                    name,
                    sale_type AS SaleType,
                    price,
                    status,
                    is_favorite AS IsFavorite,
                    note,
                    created_at AS CreatedAt,
                    updated_at AS UpdatedAt
                FROM tb_product p
                {whereSql}
                ORDER BY {orderBy} {dir};";

            var cmd = new CommandDefinition(sql, p, cancellationToken: ct);
            return (await conn.QueryAsync<Product>(cmd)).AsList();
        }

        public async Task<Product?> GetByIdAsync(long productId, CancellationToken ct = default)
        {
            using var conn = _connFactory();
            if (conn.State != ConnectionState.Open)
                conn.Open();

            var cmd = new CommandDefinition(
                SqlProductQuery.GET_PRODUCT_BY_ID,
                new { ProductId = productId },
                cancellationToken: ct);

            return await conn.QuerySingleOrDefaultAsync<Product>(cmd);
        }

        public async Task<long> InsertProductAsync(Product product, CancellationToken ct = default)
        {
            using var conn = _connFactory();
            if (conn.State != ConnectionState.Open)
                conn.Open();

            var cmd = new CommandDefinition(
                SqlProductQuery.INSERT_PRODUCT,
                product,
                cancellationToken: ct);

            return await conn.ExecuteScalarAsync<long>(cmd);
        }

        public async Task<long> InsertProductAsync(SQLiteConnection conn, SQLiteTransaction tx, Product product, CancellationToken ct = default)
        {
            var cmd = new CommandDefinition(
                SqlProductQuery.INSERT_PRODUCT,
                product,
                transaction: tx,
                cancellationToken: ct);

            return await conn.ExecuteScalarAsync<long>(cmd);
        }

        public async Task<int> UpdateProductAsync(Product product, CancellationToken ct = default)
        {
            using var conn = _connFactory();
            if (conn.State != ConnectionState.Open)
                conn.Open();

            var cmd = new CommandDefinition(
                SqlProductQuery.UPDATE_PRODUCT,
                product,
                cancellationToken: ct);

            return await conn.ExecuteAsync(cmd);
        }

        public async Task<int> UpdateProductAsync(SQLiteConnection conn, SQLiteTransaction tx, Product product, CancellationToken ct = default)
        {
            var cmd = new CommandDefinition(
                SqlProductQuery.UPDATE_PRODUCT,
                product,
                transaction: tx,
                cancellationToken: ct);

            return await conn.ExecuteAsync(cmd);
        }

        public async Task<int> DeleteProductAsync(long productId, CancellationToken ct = default)
        {
            using var conn = _connFactory();
            if (conn.State != ConnectionState.Open)
                conn.Open();

            using var tx = conn.BeginTransaction();

            try
            {
                await DeleteProductComponentsAsync(conn, tx, productId, ct);

                var cmd = new CommandDefinition(
                    SqlProductQuery.DELETE_PRODUCT,
                    new { ProductId = productId },
                    transaction: tx,
                    cancellationToken: ct);

                int affected = await conn.ExecuteAsync(cmd);

                tx.Commit();
                return affected;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<long> InsertProductComponentAsync(ProductComponent component, CancellationToken ct = default)
        {
            using var conn = _connFactory();
            if (conn.State != ConnectionState.Open)
                conn.Open();

            var cmd = new CommandDefinition(
                SqlProductComponentQuery.INSERT_PRODUCT_COMPONENT,
                component,
                cancellationToken: ct);

            return await conn.ExecuteScalarAsync<long>(cmd);
        }

        public async Task<long> InsertProductComponentAsync(SQLiteConnection conn, SQLiteTransaction tx, ProductComponent component, CancellationToken ct = default)
        {
            var cmd = new CommandDefinition(
                SqlProductComponentQuery.INSERT_PRODUCT_COMPONENT,
                component,
                transaction: tx,
                cancellationToken: ct);

            return await conn.ExecuteScalarAsync<long>(cmd);
        }

        public async Task<int> DeleteProductComponentsAsync(long productId, CancellationToken ct = default)
        {
            using var conn = _connFactory();
            if (conn.State != ConnectionState.Open)
                conn.Open();

            var cmd = new CommandDefinition(
                SqlProductComponentQuery.DELETE_PRODUCT_COMPONENTS,
                new { ProductId = productId },
                cancellationToken: ct);

            return await conn.ExecuteAsync(cmd);
        }

        public async Task<int> DeleteProductComponentsAsync(SQLiteConnection conn, SQLiteTransaction tx, long productId, CancellationToken ct = default)
        {
            var cmd = new CommandDefinition(
                SqlProductComponentQuery.DELETE_PRODUCT_COMPONENTS,
                new { ProductId = productId },
                transaction: tx,
                cancellationToken: ct);

            return await conn.ExecuteAsync(cmd);
        }

        public async Task<IReadOnlyList<ProductComponent>> GetProductComponentsAsync(long productId, CancellationToken ct = default)
        {
            using var conn = _connFactory();
            if (conn.State != ConnectionState.Open)
                conn.Open();

            var cmd = new CommandDefinition(
                SqlProductComponentQuery.GET_PRODUCT_COMPONENTS,
                new { ProductId = productId },
                cancellationToken: ct);

            return (await conn.QueryAsync<ProductComponent>(cmd)).AsList();
        }

        public async Task<long> InsertProductWithComponentsAsync(Product product, IReadOnlyList<ProductComponent> components, CancellationToken ct = default)
        {
            using var conn = _connFactory();
            if (conn.State != ConnectionState.Open)
                conn.Open();

            using var tx = conn.BeginTransaction();

            try
            {
                var productId = await InsertProductAsync(conn, tx, product, ct);

                foreach (var component in components)
                {
                    component.ProductId = (int)productId;
                    await InsertProductComponentAsync(conn, tx, component, ct);
                }

                tx.Commit();
                return productId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<int> UpdateProductWithComponentsAsync(Product product, IReadOnlyList<ProductComponent> components, CancellationToken ct = default)
        {
            using var conn = _connFactory();
            if (conn.State != ConnectionState.Open)
                conn.Open();

            using var tx = conn.BeginTransaction();

            try
            {
                int affected = await UpdateProductAsync(conn, tx, product, ct);

                await DeleteProductComponentsAsync(conn, tx, product.Id, ct);

                foreach (var component in components)
                {
                    component.ProductId = product.Id;
                    await InsertProductComponentAsync(conn, tx, component, ct);
                }

                tx.Commit();
                return affected;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<string> GenerateAsync(ProductSaleType saleType, CancellationToken ct = default)
        {
            string prefix = saleType switch
            {
                ProductSaleType.Single => "PRD",
                ProductSaleType.Package => "PKG",
                _ => "PRD"
            };

            using var conn = _connFactory();
            if (conn.State != ConnectionState.Open)
                conn.Open();

            using var tx = conn.BeginTransaction();

            var param = new { Prefix = prefix };

            await conn.ExecuteAsync(new CommandDefinition(
                SqlCodeSequenceQuery.INSERT_IF_MISSING,
                param,
                transaction: tx,
                cancellationToken: ct));

            await conn.ExecuteAsync(new CommandDefinition(
                SqlCodeSequenceQuery.UPDATE_SEQUENCE,
                param,
                transaction: tx,
                cancellationToken: ct));

            int seq = await conn.ExecuteScalarAsync<int>(new CommandDefinition(
                SqlCodeSequenceQuery.SELECT_SEQUENCE,
                param,
                transaction: tx,
                cancellationToken: ct));

            tx.Commit();

            return $"{prefix}-{seq:0000}";
        }
    }
}
