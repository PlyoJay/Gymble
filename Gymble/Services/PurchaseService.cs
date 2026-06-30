using Gymble.Models;
using Gymble.Repositories;
using System.Data.SQLite;

namespace Gymble.Services
{
    public interface IPurchaseService
    {
        Task<int> CreatePurchaseAsync(PurchaseRequest request);
    }

    public class PurchaseService : IPurchaseService
    {
        private readonly Func<SQLiteConnection> _connFactory;
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IProductRepository _productRepository;

        public PurchaseService(
            Func<SQLiteConnection> connFactory,
            IPurchaseRepository purchaseRepository,
            IProductRepository productRepository)
        {
            _connFactory = connFactory;
            _purchaseRepository = purchaseRepository;
            _productRepository = productRepository;
        }


        public async Task<int> CreatePurchaseAsync(PurchaseRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.MemberId <= 0)
                throw new InvalidOperationException("구매 대상 회원이 올바르지 않습니다.");

            if (request.Items == null || request.Items.Count == 0)
                throw new InvalidOperationException("구매할 상품이 없습니다.");

            using var conn = _connFactory();

            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();

            using var tx = conn.BeginTransaction();

            try
            {
                var now = DateTime.Now;

                var purchaseItems = new List<(PurchaseItem Item, DateTime? SelectedStartDate)>();

                foreach (var requestItem in request.Items)
                {
                    var product = await _productRepository.GetByIdAsync(requestItem.ProductId);

                    if (product == null)
                        throw new InvalidOperationException($"상품을 찾을 수 없습니다. ProductId={requestItem.ProductId}");

                    var components = await _productRepository.GetProductComponentsAsync(product.Id);

                    if (components == null || components.Count == 0)
                    {
                        // 구성 정보가 없는 상품도 일단 구매 항목으로 저장
                        purchaseItems.Add((new PurchaseItem
                        {
                            ProductId = product.Id,
                            ProductCodeSnapshot = product.Code,
                            ProductNameSnapshot = product.Name,
                            Category = ProductCategory.Etc,
                            UsageType = null,
                            StartType = null,
                            FixedStartDate = null,
                            UnitPrice = product.Price,
                            LineAmount = product.Price,
                            UsageValue = null,
                            IsMembershipItem = false,
                            Note = requestItem.Note,
                            CreatedAt = now,
                            UpdatedAt = now
                        }, requestItem.SelectedStartDate));

                        continue;
                    }

                    foreach (var component in components)
                    {
                        purchaseItems.Add((new PurchaseItem
                        {
                            ProductId = product.Id,
                            ProductCodeSnapshot = product.Code,
                            ProductNameSnapshot = string.IsNullOrWhiteSpace(component.Name)
                                ? product.Name
                                : $"{product.Name} - {component.Name}",
                            Category = component.Category,
                            UsageType = component.UsageType,
                            StartType = component.StartType,
                            FixedStartDate = component.FixedStartDate,
                            UnitPrice = product.Price,
                            LineAmount = product.Price,
                            UsageValue = component.UsageValue,
                            IsMembershipItem = IsMembershipCategory(component.Category),
                            Note = requestItem.Note,
                            CreatedAt = now,
                            UpdatedAt = now
                        }, requestItem.SelectedStartDate));
                    }
                }

                var totalAmount = purchaseItems.Sum(x => x.Item.LineAmount);
                var discountAmount = Math.Max(0, request.DiscountAmount);
                var finalAmount = Math.Max(0, totalAmount - discountAmount);

                var purchase = new Purchase
                {
                    MemberId = request.MemberId,
                    TotalAmount = totalAmount,
                    DiscountAmount = discountAmount,
                    FinalAmount = finalAmount,
                    PaymentMethod = request.PaymentMethod,
                    Status = PurchaseStatus.Completed,
                    PurchasedAt = now,
                    Memo = request.Memo,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                var purchaseId = await _purchaseRepository.InsertPurchaseAsync(conn, tx, purchase);

                foreach (var purchaseItem in purchaseItems)
                {
                    var item = purchaseItem.Item;
                    item.PurchaseId = purchaseId;

                    var purchaseItemId = await _purchaseRepository.InsertPurchaseItemAsync(conn, tx, item);

                    if (!item.IsMembershipItem)
                        continue;

                    var membership = CreateMembership(
                        request.MemberId,
                        purchaseId,
                        purchaseItemId,
                        item,
                        purchaseItem.SelectedStartDate,
                        now);

                    await _purchaseRepository.InsertMemberMembershipAsync(conn, tx, membership);
                }

                tx.Commit();

                return purchaseId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        private static bool IsMembershipCategory(ProductCategory category)
        {
            return category is ProductCategory.Gym
                or ProductCategory.PT
                or ProductCategory.Locker
                or ProductCategory.Wear;
        }

        private static MemberMembership CreateMembership(
            int memberId,
            int purchaseId,
            int purchaseItemId,
            PurchaseItem item,
            DateTime? selectedStartDate,
            DateTime now)
        {
            var usageType = item.UsageType ?? ProductUsageType.Period;
            var startType = item.StartType ?? ProductStartType.Immediate;
            var usageValue = item.UsageValue ?? 0;

            DateTime? startDate = null;
            DateTime? endDate = null;
            DateTime? activatedAt = null;

            int? durationDays = null;
            int? totalCount = null;
            int? usedCount = null;
            int? remainingCount = null;

            var status = MembershipStatus.Pending;

            switch (startType)
            {
                case ProductStartType.Immediate:
                    startDate = now.Date;
                    activatedAt = now;
                    status = MembershipStatus.Active;
                    break;

                case ProductStartType.SelectDate:
                    if (!selectedStartDate.HasValue)
                        throw new InvalidOperationException("직접 선택 시작일 상품은 시작일이 필요합니다.");

                    startDate = selectedStartDate.Value.Date;
                    if (startDate.Value <= now.Date)
                    {
                        activatedAt = now;
                        status = MembershipStatus.Active;
                    }
                    break;

                case ProductStartType.FirstCheckIn:
                    startDate = null;
                    activatedAt = null;
                    status = MembershipStatus.Pending;
                    break;

                case ProductStartType.FixedDate:
                    if (!item.FixedStartDate.HasValue)
                        throw new InvalidOperationException("고정 시작일 상품은 FixedStartDate가 필요합니다.");

                    startDate = item.FixedStartDate.Value.Date;
                    if (startDate.Value <= now.Date)
                    {
                        activatedAt = now;
                        status = MembershipStatus.Active;
                    }
                    break;
            }

            if (usageType == ProductUsageType.Period)
            {
                durationDays = usageValue;

                if (startDate.HasValue && usageValue > 0)
                {
                    endDate = startDate.Value.AddDays(usageValue);
                }
            }
            else if (usageType == ProductUsageType.Count)
            {
                totalCount = usageValue;
                usedCount = 0;
                remainingCount = usageValue;
            }

            return new MemberMembership
            {
                MemberId = memberId,
                PurchaseId = purchaseId,
                PurchaseItemId = purchaseItemId,
                ProductId = item.ProductId,
                ProductCodeSnapshot = item.ProductCodeSnapshot,
                ProductNameSnapshot = item.ProductNameSnapshot,
                Category = item.Category,
                UsageType = usageType,
                StartType = startType,
                UnitPriceSnapshot = item.UnitPrice,
                UsageValue = usageValue,
                DurationDays = durationDays,
                TotalCount = totalCount,
                UsedCount = usedCount,
                RemainingCount = remainingCount,
                PurchasedAt = now,
                ActivatedAt = activatedAt,
                StartDate = startDate,
                EndDate = endDate,
                Status = status,
                Note = item.Note,
                CreatedAt = now,
                UpdatedAt = now
            };
        }
    }
}
