using Gymble.Models;
using Gymble.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gymble.Services
{
    public interface IProductService
    {
        Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Product>> SearchAsync(ProductSearch q, CancellationToken ct = default);
        Task<Product?> GetByIdAsync(long productId, CancellationToken ct = default);
        Task<long> AddAsync(Product product, CancellationToken ct = default);
        Task UpdateAsync (Product product, CancellationToken ct = default);
        Task DeleteAsync(long productId, CancellationToken ct = default);
    }

    public sealed class ProductServices : IProductService
    {
        private readonly IProductService _repo;

        public ProductServices(IProductService repo)
            => _repo = repo ?? throw new ArgumentNullException(nameof(repo));

        public Task<long> AddAsync(Product product, CancellationToken ct = default)
        {
            return _repo.AddAsync(product, ct);
        }

        public Task DeleteAsync(long productId, CancellationToken ct = default)
        {
            return _repo.DeleteAsync(productId, ct);
        }

        public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default)
        {
            return _repo.GetAllAsync(ct);
        }

        public Task<Product?> GetByIdAsync(long productId, CancellationToken ct = default)
        {
            return _repo.GetByIdAsync(productId, ct);
        }

        public Task<IReadOnlyList<Product>> SearchAsync(ProductSearch q, CancellationToken ct = default)
        {
            return _repo.SearchAsync(q, ct);
        }

        public Task UpdateAsync(Product product, CancellationToken ct = default)
        {
            return _repo.UpdateAsync(product, ct);
        }
    }
}
