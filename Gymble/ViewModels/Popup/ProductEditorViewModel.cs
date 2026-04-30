using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gymble.Models;
using Gymble.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

public enum ProductEditorMode
{
    Add,
    Edit
}

namespace Gymble.ViewModels.Popup
{
    public partial class ProductComponentEditorItem : ObservableObject
    {
        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private ProductCategory category = ProductCategory.Gym;

        [ObservableProperty]
        private ProductUsageType usageType = ProductUsageType.Period;

        [ObservableProperty]
        private int usageValue = 1;

        [ObservableProperty]
        private ProductStartType startType = ProductStartType.Immediate;

        [ObservableProperty]
        private DateTime? fixedStartDate;

        [ObservableProperty]
        private string? note;
    }

    public partial class ProductEditorViewModel : ObservableObject
    {
        private readonly IProductService _productService;
        private readonly ProductEditorMode _mode;
        private readonly Product? _originalProduct;

        public bool IsAddMode => _mode == ProductEditorMode.Add;
        public bool IsEditMode => _mode == ProductEditorMode.Edit;

        public string Title => IsAddMode ? "상품 등록" : "상품 수정";
        public string SaveButtonText => IsAddMode ? "추가" : "저장";

        public event Action<bool>? RequestClose;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private string productName = string.Empty;

        [ObservableProperty]
        private bool isAutoCode = false;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private string productCode = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private ProductSaleType selectedSaleType = ProductSaleType.Single;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private int price = 0;

        [ObservableProperty]
        private ProductStatus selectedStatus = ProductStatus.OnSale;

        [ObservableProperty]
        private bool isFavorite;

        [ObservableProperty]
        private string? note;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private bool isBusy = false;

        public bool IsCodeReadOnly => IsAutoCode;

        public ObservableCollection<ProductComponentEditorItem> Components { get; } = new();

        public IEnumerable<ProductSaleType> SaleTypes { get; } =
            Enum.GetValues(typeof(ProductSaleType)).Cast<ProductSaleType>();

        public IEnumerable<ProductCategory> Categories { get; } =
            Enum.GetValues(typeof(ProductCategory)).Cast<ProductCategory>();

        public IEnumerable<ProductStartType> StartTypes { get; } =
            Enum.GetValues(typeof(ProductStartType)).Cast<ProductStartType>();

        public ICommand CloseCommand { get; }
        public IAsyncRelayCommand SaveCommand { get; }
        public IRelayCommand AddComponentCommand { get; }
        public IRelayCommand<ProductComponentEditorItem> RemoveComponentCommand { get; }

        public ProductEditorViewModel(IProductService productService)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _mode = ProductEditorMode.Add;

            CloseCommand = new RelayCommand(() => Close(false));
            SaveCommand = new AsyncRelayCommand(SaveProductAsync, CanSave);
            AddComponentCommand = new RelayCommand(AddComponent);
            RemoveComponentCommand = new RelayCommand<ProductComponentEditorItem>(RemoveComponent);

            // 현재 단계에서는 단품 기준 시작.
            // 통합권 UI가 완성되기 전까지 기본 구성품 1개를 만들어 둔다.
            Components.Add(CreateDefaultComponent());
        }

        public ProductEditorViewModel(IProductService productService, Product product)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _mode = ProductEditorMode.Edit;
            _originalProduct = product ?? throw new ArgumentNullException(nameof(product));

            CloseCommand = new RelayCommand(() => Close(false));
            SaveCommand = new AsyncRelayCommand(SaveProductAsync, CanSave);
            AddComponentCommand = new RelayCommand(AddComponent);
            RemoveComponentCommand = new RelayCommand<ProductComponentEditorItem>(RemoveComponent);

            _ = LoadFromAsync(product);
        }

        partial void OnIsAutoCodeChanged(bool value)
        {
            OnPropertyChanged(nameof(IsCodeReadOnly));

            if (value)
                ProductCode = string.Empty;
        }

        private async Task LoadFromAsync(Product product)
        {
            try
            {
                IsBusy = true;

                ProductName = product.Name;
                ProductCode = product.Code;
                SelectedSaleType = product.SaleType;
                Price = product.Price;
                SelectedStatus = product.Status;
                IsFavorite = product.IsFavorite;
                Note = product.Note;
                IsAutoCode = false;

                Components.Clear();

                var components = await _productService.GetComponentsAsync(product.Id);

                foreach (var c in components)
                {
                    Components.Add(new ProductComponentEditorItem
                    {
                        Name = c.Name,
                        Category = c.Category,
                        UsageType = c.UsageType,
                        UsageValue = c.UsageValue,
                        StartType = c.StartType,
                        FixedStartDate = c.FixedStartDate,
                        Note = c.Note
                    });
                }

                if (Components.Count == 0)
                {
                    Components.Add(CreateDefaultComponent());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"상품 정보를 불러오지 못했습니다.\n{ex.Message}");
            }
            finally
            {
                IsBusy = false;
                SaveCommand.NotifyCanExecuteChanged();
            }
        }

        private async Task SaveProductAsync()
        {
            if (!CanSave())
            {
                MessageBox.Show("필수 입력값을 확인해주세요.");
                return;
            }

            try
            {
                IsBusy = true;

                var request = CreateUpsertRequest(IsEditMode ? _originalProduct?.Id : null);

                if (IsAddMode)
                    await _productService.AddAsync(request);
                else
                    await _productService.UpdateAsync(request);

                IsBusy = false;

                Close(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"상품 저장 중 오류가 발생했습니다.\n{ex.Message}");
            }
            finally
            {
                IsBusy = false;
                SaveCommand.NotifyCanExecuteChanged();
            }
        }

        private ProductUpsertRequest CreateUpsertRequest(int? id = null)
        {
            return new ProductUpsertRequest
            {
                Id = id,
                Name = ProductName.Trim(),
                Code = IsAutoCode ? "" : ProductCode.Trim(),
                SaleType = SelectedSaleType,
                Price = Price,
                Status = SelectedStatus,
                IsFavorite = IsFavorite,
                Note = Note,
                Components = Components.Select(x => new ProductComponent
                {
                    Name = x.Name.Trim(),
                    Category = x.Category,
                    UsageType = x.UsageType,
                    UsageValue = x.UsageValue,
                    StartType = x.StartType,
                    FixedStartDate = x.StartType == ProductStartType.FixedDate ? x.FixedStartDate : null,
                    Note = x.Note
                }).ToList()
            };
        }

        private ProductComponentEditorItem CreateDefaultComponent()
        {
            return new ProductComponentEditorItem
            {
                Name = "기본 구성품",
                Category = ProductCategory.Gym,
                UsageType = ProductUsageType.Period,
                UsageValue = 1,
                StartType = ProductStartType.Immediate,
                FixedStartDate = null,
                Note = null
            };
        }

        private void AddComponent()
        {
            Components.Add(CreateDefaultComponent());
            SaveCommand.NotifyCanExecuteChanged();
        }

        private void RemoveComponent(ProductComponentEditorItem? item)
        {
            if (item == null)
                return;

            if (Components.Count == 1)
            {
                MessageBox.Show("구성품은 최소 1개 이상 필요합니다.");
                return;
            }

            Components.Remove(item);
            SaveCommand.NotifyCanExecuteChanged();
        }

        private void Close(bool result)
        {
            if (IsBusy)
                return;

            RequestClose?.Invoke(result);
        }

        private bool CanSave()
        {
            if (IsBusy) return false;
            if (string.IsNullOrWhiteSpace(ProductName)) return false;
            if (!IsAutoCode && string.IsNullOrWhiteSpace(ProductCode)) return false;
            if (Price <= 0) return false;
            if (Components.Count == 0) return false;

            if (SelectedSaleType == ProductSaleType.Single && Components.Count != 1)
                return false;

            foreach (var c in Components)
            {
                if (string.IsNullOrWhiteSpace(c.Name)) return false;
                if (c.UsageValue <= 0) return false;
                if (c.StartType == ProductStartType.FixedDate && !c.FixedStartDate.HasValue)
                    return false;
            }

            return true;
        }
    }
}