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
    public interface IProductRepository
    {
        Task<IReadOnlyList<Member>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Product>> SearchAsync(ProductSearch q, CancellationToken ct = default);
        Task<Product?> GetByIdAsync(long productId, CancellationToken ct = default);
        Task<long> InsertProductAsync(Product prodcut, CancellationToken ct = default);
        Task<int> UpdateProductAsync(Product product, CancellationToken ct = default);
        Task<int> DeleteProductAsync(long productId, CancellationToken ct = default);
    }

    public class ProductRepository : IProductRepository
    {
        private readonly Func<SQLiteConnection> _connFactory;

        public ProductRepository(Func<SQLiteConnection> connFactory)
            => _connFactory = connFactory;

        public Task<IReadOnlyList<Member>> GetAllAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Product>> SearchAsync(ProductSearch q, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<Product?> GetByIdAsync(long productId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<long> InsertProductAsync(Product product, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }


        public Task<int> UpdateProductAsync(Product product, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeleteProductAsync(long productId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
