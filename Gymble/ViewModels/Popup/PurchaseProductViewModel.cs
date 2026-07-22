using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gymble.Models;
using Gymble.Services;
using System.Collections.ObjectModel;

namespace Gymble.ViewModels.Popup
{
    public partial class PurchaseProductViewModel : ObservableObject
    {
        private readonly IPurchaseService _purchaseService;
        private readonly IProductService _productService;
        private CancellationTokenSource? _componentLoadCts;

        public event Action<bool?>? CloseRequested;
        public event Action<string>? MessageRequested;

        public ObservableCollection<Product> Products { get; } = new();
        public ObservableCollection<ProductComponent> SelectedProductComponents { get; } = new();

        public IEnumerable<PaymentMethod> PaymentMethods { get; } =
            Enum.GetValues(typeof(PaymentMethod)).Cast<PaymentMethod>();

        [ObservableProperty]
        private Member? targetMember;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        private DateTime? selectedDate;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        [NotifyPropertyChangedFor(nameof(ProductPrice))]
        [NotifyPropertyChangedFor(nameof(FinalAmount))]
        private Product? selectedProduct;

        [ObservableProperty]
        private PaymentMethod selectedPaymentMethod = PaymentMethod.Card;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        [NotifyPropertyChangedFor(nameof(DiscountAmount))]
        [NotifyPropertyChangedFor(nameof(FinalAmount))]
        private string discountAmountText = "0";

        [ObservableProperty]
        private string? memo;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        private bool isBusy;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        private string? errorMessage;

        [ObservableProperty]
        private bool isStartDateVisible;

        [ObservableProperty]
        private bool isStartDateEditable;

        [ObservableProperty]
        private string startDateLabel = "시작 날짜";

        [ObservableProperty]
        private string componentStartSummary = "";

        public int ProductPrice => SelectedProduct?.Price ?? 0;
        public int DiscountAmount => TryGetDiscountAmount(out var amount) ? amount : 0;
        public int FinalAmount => Math.Max(0, ProductPrice - DiscountAmount);

        public PurchaseProductViewModel(IPurchaseService purchaseService, IProductService productService)
        {
            _purchaseService = purchaseService ?? throw new ArgumentNullException(nameof(purchaseService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        }

        public void Initialize(Member? member)
        {
            if (member == null)
            {
                TargetMember = null;
                ErrorMessage = "구매 대상 회원이 올바르지 않습니다.";
                return;
            }

            TargetMember = new Member
            {
                Id = member.Id,
                Name = member.Name,
                Gender = member.Gender,
                PhoneNumber = member.PhoneNumber,
                BirthDate = member.BirthDate,
                RegisterDate = member.RegisterDate,
                Status = member.Status,
                Memo = member.Memo
            };

            ErrorMessage = null;
            _ = LoadProductsAsync();
        }

        partial void OnSelectedProductChanged(Product? value)
        {
            ValidateForm();
            _ = LoadSelectedProductComponentsAsync(value?.Id);
        }

        partial void OnSelectedDateChanged(DateTime? value)
        {
            ValidateForm();
        }

        partial void OnDiscountAmountTextChanged(string value)
        {
            var digits = new string((value ?? "").Where(char.IsDigit).ToArray());
            var normalized = string.IsNullOrEmpty(digits) ? "0" : digits;

            if (value != normalized)
            {
                DiscountAmountText = normalized;
                return;
            }

            ValidateForm();
        }

        [RelayCommand(CanExecute = nameof(CanRegister))]
        private async Task RegisterAsync()
        {
            if (IsBusy || !ValidateForm())
                return;

            if (TargetMember == null || SelectedProduct == null)
                return;

            IsBusy = true;
            ErrorMessage = null;

            try
            {
                var request = new PurchaseRequest
                {
                    MemberId = TargetMember.Id,
                    PaymentMethod = SelectedPaymentMethod,
                    DiscountAmount = DiscountAmount,
                    Memo = Memo,
                    Items =
                    {
                        new PurchaseRequestItem
                        {
                            ProductId = SelectedProduct.Id,
                            SelectedStartDate = IsStartDateEditable ? SelectedDate : null
                        }
                    }
                };

                await _purchaseService.CreatePurchaseAsync(request);
                MessageRequested?.Invoke("구매 등록이 완료되었습니다.");
                CloseRequested?.Invoke(true);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            CloseRequested?.Invoke(false);
        }

        private bool CanRegister()
        {
            return !IsBusy
                && TargetMember != null
                && SelectedProduct != null
                && string.IsNullOrWhiteSpace(ErrorMessage);
        }

        private async Task LoadProductsAsync()
        {
            IsBusy = true;
            ErrorMessage = null;

            try
            {
                Products.Clear();

                var products = await _productService.SearchAsync(new ProductSearch
                {
                    Statuses = new List<ProductStatus> { ProductStatus.OnSale },
                    SortBy = "name",
                    Desc = false
                });

                foreach (var product in products)
                    Products.Add(product);

                SelectedProduct = Products.FirstOrDefault();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
                RegisterCommand.NotifyCanExecuteChanged();
            }
        }

        private async Task LoadSelectedProductComponentsAsync(int? productId)
        {
            _componentLoadCts?.Cancel();
            _componentLoadCts?.Dispose();
            _componentLoadCts = new CancellationTokenSource();
            var ct = _componentLoadCts.Token;

            SelectedProductComponents.Clear();
            IsStartDateVisible = false;
            IsStartDateEditable = false;
            SelectedDate = null;
            ComponentStartSummary = "";

            if (!productId.HasValue || productId.Value <= 0)
                return;

            try
            {
                var components = await _productService.GetComponentsAsync(productId.Value, ct);

                if (ct.IsCancellationRequested || SelectedProduct?.Id != productId.Value)
                    return;

                foreach (var component in components)
                    SelectedProductComponents.Add(component);

                ApplyStartDateState(components);
                ValidateForm();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        private void ApplyStartDateState(IReadOnlyList<ProductComponent> components)
        {
            if (components.Count == 0)
            {
                ComponentStartSummary = "구성품 없음";
                return;
            }

            var startTypes = components.Select(x => x.StartType).Distinct().ToList();
            ComponentStartSummary = string.Join(", ", components.Select(x => $"{x.Name}: {ToStartTypeText(x.StartType)}"));

            if (startTypes.Contains(ProductStartType.SelectDate))
            {
                IsStartDateVisible = true;
                IsStartDateEditable = true;
                StartDateLabel = "시작 날짜";
                SelectedDate = DateTime.Today;
                return;
            }

            if (startTypes.Contains(ProductStartType.FixedDate))
            {
                IsStartDateVisible = true;
                IsStartDateEditable = false;
                StartDateLabel = "고정 시작일";
                SelectedDate = components.First(x => x.StartType == ProductStartType.FixedDate).FixedStartDate;
                return;
            }

            IsStartDateVisible = false;
            IsStartDateEditable = false;
            SelectedDate = null;
        }

        private bool ValidateForm()
        {
            string? error = null;

            if (TargetMember == null)
                error = "구매 대상 회원이 올바르지 않습니다.";
            else if (SelectedProduct == null)
                error = "상품을 선택해 주세요.";
            else if (!TryGetDiscountAmount(out var discountAmount))
                error = "할인금액은 숫자만 입력해 주세요.";
            else if (discountAmount < 0)
                error = "할인금액은 0원보다 작을 수 없습니다.";
            else if (discountAmount > ProductPrice)
                error = "할인금액은 상품 정상가보다 클 수 없습니다.";
            else if (IsStartDateEditable && !SelectedDate.HasValue)
                error = "시작일을 선택해 주세요.";
            else if (IsStartDateEditable && SelectedDate is { } selectedDate && selectedDate.Date < DateTime.Today)
                error = "시작일은 오늘보다 이전일 수 없습니다.";

            ErrorMessage = error;
            return error == null;
        }

        private bool TryGetDiscountAmount(out int amount)
        {
            amount = 0;

            if (string.IsNullOrWhiteSpace(DiscountAmountText))
                return true;

            return int.TryParse(DiscountAmountText, out amount);
        }

        private static string ToStartTypeText(ProductStartType startType)
        {
            return startType switch
            {
                ProductStartType.Immediate => "결제 즉시",
                ProductStartType.SelectDate => "직접 선택",
                ProductStartType.FirstCheckIn => "첫 출석 시작",
                ProductStartType.FixedDate => "고정 날짜",
                _ => startType.ToString()
            };
        }
    }
}
