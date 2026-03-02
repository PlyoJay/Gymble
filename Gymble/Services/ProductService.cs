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

    public sealed class ProductService : IProductService
    {
        private readonly IProductRepository _repo;

        public ProductService(IProductRepository repo)
            => _repo = repo ?? throw new ArgumentNullException(nameof(repo));

        public Task<long> AddAsync(Product product, CancellationToken ct = default)
        {
            Validate(product, true);

            if (product.CreatedAt == default)
                product.CreatedAt = DateTime.Now;

            if (product.UpdatedAt == default)
                product.UpdatedAt = DateTime.Now;

            return _repo.InsertProductAsync(product, ct);
        }

        public Task DeleteAsync(long productId, CancellationToken ct = default)
        {
            return _repo.DeleteProductAsync(productId, ct);
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
            Validate(product, isNew: false);

            return _repo.UpdateProductAsync(product, ct);
        }

        private static void Validate(Product product, bool isNew)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));

            if (!isNew && product.Id <= 0)
                throw new ArgumentException("상품 ID가 올바르지 않습니다.");

            if (string.IsNullOrEmpty(product.Name))
                throw new ArgumentException("상품명은 필수입니다.");

            if (string.IsNullOrEmpty(product.Code))
                throw new ArgumentException("상품코드는 필수입니다.");

            if (product.Price < 0)
                throw new ArgumentException("가격은 0원보다 작을 수 없습니다.");
        }
    }
}
