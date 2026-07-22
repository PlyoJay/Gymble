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
        private readonly IMemberRepository _memberRepository;

        public PurchaseService(
            Func<SQLiteConnection> connFactory,
            IPurchaseRepository purchaseRepository,
            IProductRepository productRepository,
            IMemberRepository memberRepository)
        {
            _connFactory = connFactory;
            _purchaseRepository = purchaseRepository;
            _productRepository = productRepository;
            _memberRepository = memberRepository;
        }

        public async Task<int> CreatePurchaseAsync(PurchaseRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var now = DateTime.Now;
            var purchaseItems = await BuildValidatedPurchaseItemsAsync(request, now);
            var totalAmount = purchaseItems.Sum(x => x.Item.LineAmount);
            var discountAmount = request.DiscountAmount;
            var finalAmount = totalAmount - discountAmount;

            using var conn = _connFactory();

            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();

            using var tx = conn.BeginTransaction();

            try
            {
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

        private async Task<List<(PurchaseItem Item, DateTime? SelectedStartDate)>> BuildValidatedPurchaseItemsAsync(
            PurchaseRequest request,
            DateTime now)
        {
            if (request.MemberId <= 0)
                throw new InvalidOperationException("구매 대상 회원이 올바르지 않습니다.");

            if (!await _memberRepository.ExistsAsync(request.MemberId))
                throw new InvalidOperationException("구매 대상 회원을 찾을 수 없습니다.");

            if (request.Items == null || request.Items.Count == 0)
                throw new InvalidOperationException("구매할 상품을 선택해 주세요.");

            if (request.DiscountAmount < 0)
                throw new InvalidOperationException("할인금액은 0원보다 작을 수 없습니다.");

            var purchaseItems = new List<(PurchaseItem Item, DateTime? SelectedStartDate)>();
            var totalAmount = 0;

            foreach (var requestItem in request.Items)
            {
                if (requestItem.ProductId <= 0)
                    throw new InvalidOperationException("선택한 상품 정보가 올바르지 않습니다.");

                var product = await _productRepository.GetByIdAsync(requestItem.ProductId);

                if (product == null)
                    throw new InvalidOperationException("선택한 상품을 찾을 수 없습니다.");

                if (product.Status != ProductStatus.OnSale)
                    throw new InvalidOperationException($"'{product.Name}' 상품은 현재 판매 중이 아닙니다.");

                var components = await _productRepository.GetProductComponentsAsync(product.Id);

                if (components == null || components.Count == 0)
                    throw new InvalidOperationException($"'{product.Name}' 상품에 구성품이 없어 구매할 수 없습니다. 상품 구성을 먼저 확인해 주세요.");

                ValidateSelectedStartDate(product, components, requestItem.SelectedStartDate, now);

                for (var i = 0; i < components.Count; i++)
                {
                    var component = components[i];
                    ValidateComponent(product, component);

                    var lineAmount = i == 0 ? product.Price : 0;
                    totalAmount += lineAmount;

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
                        LineAmount = lineAmount,
                        UsageValue = component.UsageValue,
                        IsMembershipItem = IsMembershipCategory(component.Category),
                        Note = requestItem.Note,
                        CreatedAt = now,
                        UpdatedAt = now
                    }, requestItem.SelectedStartDate));
                }
            }

            if (request.DiscountAmount > totalAmount)
                throw new InvalidOperationException("할인금액은 상품 정상가보다 클 수 없습니다.");

            return purchaseItems;
        }

        private static void ValidateSelectedStartDate(
            Product product,
            IReadOnlyList<ProductComponent> components,
            DateTime? selectedStartDate,
            DateTime now)
        {
            if (!components.Any(x => x.StartType == ProductStartType.SelectDate))
                return;

            if (!selectedStartDate.HasValue)
                throw new InvalidOperationException($"'{product.Name}' 상품은 시작일을 선택해야 합니다.");

            if (selectedStartDate.Value.Date < now.Date)
                throw new InvalidOperationException("선택 시작일은 오늘보다 이전일 수 없습니다.");
        }

        private static void ValidateComponent(Product product, ProductComponent component)
        {
            if (component.StartType == ProductStartType.FixedDate && !component.FixedStartDate.HasValue)
                throw new InvalidOperationException($"'{product.Name}' 상품의 고정 시작일이 설정되어 있지 않습니다.");

            if (component.UsageType == ProductUsageType.Period && component.UsageValue < 1)
                throw new InvalidOperationException($"'{product.Name} - {component.Name}' 기간제 구성의 사용 기간은 1일 이상이어야 합니다.");

            if (component.UsageType == ProductUsageType.Count && component.UsageValue < 1)
                throw new InvalidOperationException($"'{product.Name} - {component.Name}' 횟수제 구성의 사용 횟수는 1회 이상이어야 합니다.");
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
                    endDate = startDate.Value.AddDays(usageValue);
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
