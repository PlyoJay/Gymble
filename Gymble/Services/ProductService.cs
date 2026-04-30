using Dapper;
using Gymble.Models;
using Gymble.Repositories;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gymble.Services
{
    public interface IProductService
    {
        Task<IReadOnlyList<Product>> SearchAsync(ProductSearch q, CancellationToken ct = default);
        Task<Product?> GetByIdAsync(long productId, CancellationToken ct = default);
        Task<IReadOnlyList<ProductComponent>> GetComponentsAsync(long productId, CancellationToken ct = default);

        Task<long> AddAsync(ProductUpsertRequest request, CancellationToken ct = default);
        Task UpdateAsync(ProductUpsertRequest request, CancellationToken ct = default);

        Task DeleteAsync(long productId, CancellationToken ct = default);
    }

    public sealed class ProductService : IProductService
    {
        private readonly IProductRepository _repo;

        public ProductService(IProductRepository repo)
            => _repo = repo ?? throw new ArgumentNullException(nameof(repo));

        public Task<IReadOnlyList<Product>> SearchAsync(ProductSearch q, CancellationToken ct = default)
        {
            q ??= new ProductSearch();

            if (string.IsNullOrWhiteSpace(q.SortBy))
                q.SortBy = "created_at";

            return _repo.SearchAsync(q, ct);
        }

        public Task<Product?> GetByIdAsync(long productId, CancellationToken ct = default)
        {
            return _repo.GetByIdAsync(productId, ct);
        }

        public Task<IReadOnlyList<ProductComponent>> GetComponentsAsync(long productId, CancellationToken ct = default)
        {
            return _repo.GetProductComponentsAsync(productId, ct);
        }

        public async Task<long> AddAsync(ProductUpsertRequest request, CancellationToken ct = default)
        {
            Validate(request, isNew: true);

            var now = DateTime.Now;

            var product = new Product
            {
                Code = request.Code,
                Name = request.Name,
                SaleType = request.SaleType,
                Price = request.Price,
                Status = request.Status,
                IsFavorite = request.IsFavorite,
                Note = request.Note,
                CreatedAt = now,
                UpdatedAt = now
            };

            if (string.IsNullOrWhiteSpace(product.Code))
                product.Code = await _repo.GenerateAsync(product.SaleType, ct);

            return await _repo.InsertProductWithComponentsAsync(product, request.Components, ct);
        }

        public async Task UpdateAsync(ProductUpsertRequest request, CancellationToken ct = default)
        {
            Validate(request, isNew: false);

            var product = new Product
            {
                Id = request.Id!.Value,
                Code = request.Code,
                Name = request.Name,
                SaleType = request.SaleType,
                Price = request.Price,
                Status = request.Status,
                IsFavorite = request.IsFavorite,
                Note = request.Note,
                UpdatedAt = DateTime.Now
            };

            await _repo.UpdateProductWithComponentsAsync(product, request.Components, ct);
        }

        public Task DeleteAsync(long productId, CancellationToken ct = default)
        {
            return _repo.DeleteProductAsync(productId, ct);
        }

        private static void Validate(ProductUpsertRequest request, bool isNew)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (!isNew && (!request.Id.HasValue || request.Id.Value <= 0))
                throw new ArgumentException("상품 ID가 올바르지 않습니다.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("상품명은 필수입니다.");

            if (request.Price < 0)
                throw new ArgumentException("가격은 0원보다 작을 수 없습니다.");

            if (request.Components == null || request.Components.Count == 0)
                throw new ArgumentException("상품 구성품은 최소 1개 이상이어야 합니다.");

            if (request.SaleType == ProductSaleType.Single && request.Components.Count != 1)
                throw new ArgumentException("단품 상품은 구성품이 정확히 1개여야 합니다.");

            foreach (var component in request.Components)
            {
                if (string.IsNullOrWhiteSpace(component.Name))
                    throw new ArgumentException("구성품명은 필수입니다.");

                if (component.UsageValue <= 0)
                    throw new ArgumentException("구성품 사용값은 1 이상이어야 합니다.");

                if (component.StartType == ProductStartType.FixedDate && !component.FixedStartDate.HasValue)
                    throw new ArgumentException("고정 시작일 상품은 FixedStartDate가 필요합니다.");
            }
        }
    }
}
