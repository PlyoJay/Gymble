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

        public event Action<bool?>? CloseRequested;

        public ObservableCollection<Product> Products { get; } = new();

        public IEnumerable<PaymentMethod> PaymentMethods { get; } =
            Enum.GetValues(typeof(PaymentMethod)).Cast<PaymentMethod>();

        [ObservableProperty]
        private ProductCategory selectedCategory = ProductCategory.Gym;

        [ObservableProperty]
        private Member? targetMember;

        [ObservableProperty]
        private DateTime selectedDate = DateTime.Today;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        private Product? selectedProduct;

        [ObservableProperty]
        private PaymentMethod selectedPaymentMethod = PaymentMethod.Card;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        private int discountAmount;

        [ObservableProperty]
        private string? memo;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
        private bool isBusy;

        [ObservableProperty]
        private string? errorMessage;

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

        [RelayCommand(CanExecute = nameof(CanRegister))]
        private async Task RegisterAsync()
        {
            ErrorMessage = null;

            if (TargetMember == null)
            {
                ErrorMessage = "구매 대상 회원이 올바르지 않습니다.";
                return;
            }

            if (SelectedProduct == null)
            {
                ErrorMessage = "상품을 선택하세요.";
                return;
            }

            if (DiscountAmount < 0)
            {
                ErrorMessage = "할인금액은 0원보다 작을 수 없습니다.";
                return;
            }

            IsBusy = true;

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
                            SelectedStartDate = SelectedDate
                        }
                    }
                };

                await _purchaseService.CreatePurchaseAsync(request);
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
            return !IsBusy;
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
            }
        }
    }
}
