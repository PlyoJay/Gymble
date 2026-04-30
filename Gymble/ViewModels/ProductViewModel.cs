using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gymble.Models;
using Gymble.Services;
using Gymble.Utils;
using Gymble.ViewModels.Popup;
using Gymble.Views.Popup;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Gymble.ViewModels
{
    public partial class StatusItem : ObservableObject
    {
        public ProductStatus Status { get; set; }
        public string Name { get; set; } = string.Empty;

        [ObservableProperty]
        private bool isChecked;
    }

    public partial class ProductViewModel : ObservableObject
    {
        public string PageTitle { get; set; } = "상품 관리";

        public ObservableCollection<Product> Items { get; } = new();

        public ProductSearch CurrentSearch { get; private set; } = new();

        public ObservableCollection<StatusItem> StatusFilters { get; } =
        [
            new StatusItem { Status = ProductStatus.OnSale, Name=ProductStatus.OnSale.GetEnumDescription(), IsChecked=true},
            new StatusItem { Status = ProductStatus.Stopped, Name=ProductStatus.Stopped.GetEnumDescription()},
            new StatusItem { Status = ProductStatus.Discontinued, Name=ProductStatus.Discontinued.GetEnumDescription()}
        ];

        public IEnumerable<ProductUsageType> UsageTypes
            => Enum.GetValues(typeof(ProductUsageType)).Cast<ProductUsageType>();

        [ObservableProperty]
        private string searchInput = string.Empty;

        [ObservableProperty]
        private ProductCategory selectedCategory = ProductCategory.Gym;

        [ObservableProperty]
        private ProductUsageType selectedUsageType;

        [ObservableProperty]
        private string minUsageValue = string.Empty;

        [ObservableProperty]
        private string maxUsageValue = string.Empty;

        [ObservableProperty]
        private string minPrice = string.Empty;

        [ObservableProperty]
        private string maxPrice = string.Empty;

        [ObservableProperty]
        private Product? selectedProduct;

        [ObservableProperty]
        private string selectedProductInfo = NO_INFO_TEXT;

        private string componentSummary = NO_INFO_TEXT;
        public string ComponentSummary
        {
            get => componentSummary;
            private set => SetProperty(ref componentSummary, value);
        }

        [ObservableProperty]
        private int totalCount;

        partial void OnSelectedCategoryChanged(ProductCategory value)
        {
            // TODO(ProductComponent): 구성품 JOIN 검색이 추가되면 Category 필터를 다시 연결한다.
        }

        partial void OnSelectedUsageTypeChanged(ProductUsageType value)
        {
            // TODO(ProductComponent): 구성품 JOIN 검색이 추가되면 UsageType 필터를 다시 연결한다.
        }

        partial void OnMinUsageValueChanged(string value)
        {
            // TODO(ProductComponent): 구성품 JOIN 검색이 추가되면 MinUsageValue 필터를 다시 연결한다.
        }

        partial void OnMaxUsageValueChanged(string value)
        {
            // TODO(ProductComponent): 구성품 JOIN 검색이 추가되면 MaxUsageValue 필터를 다시 연결한다.
        }

        partial void OnMinPriceChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                CurrentSearch.MinPrice = null;
                SearchProduct();
                return;
            }

            if (!int.TryParse(value, out int minPrice))
                return;

            if (int.TryParse(MaxPrice, out int maxPrice) && minPrice > maxPrice)
                return;

            CurrentSearch.MinPrice = minPrice;
            SearchProduct();
        }

        partial void OnMaxPriceChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                CurrentSearch.MaxPrice = null;
                SearchProduct();
                return;
            }

            if (!int.TryParse(value, out int maxPrice))
                return;

            if (int.TryParse(MinPrice, out int minPrice) && minPrice > maxPrice)
                return;

            CurrentSearch.MaxPrice = maxPrice;
            SearchProduct();
        }

        partial void OnSelectedProductChanged(Product? value)
        {
            if (value == null)
            {
                SelectedProductInfo = NO_INFO_TEXT;
                ComponentSummary = NO_INFO_TEXT;
                return;
            }

            ComponentSummary = "불러오는 중...";
            SelectedProductInfo = CreateProductInfoText(value);

            _ = LoadComponentSummaryAsync(value);
        }

        public ICommand? SearchCommand { get; }
        public ICommand? ResetFilterCommand { get; }
        public IAsyncRelayCommand? AddCommand { get; }
        public ICommand? EditCommand { get; }
        public ICommand? StopCommand { get; }
        public ICommand? DeleteCommand { get; }

        #region Fields

        private readonly IProductService _productService;

        #endregion

        public Action? RequestPage { get; set; }

        private const string NO_INFO_TEXT = "없음";

        private bool _isUpdating;

        public ProductViewModel(IProductService productService)
        {
            _productService = productService;

            SearchCommand = new RelayCommand(SearchProduct);
            ResetFilterCommand = new RelayCommand(ResetFilters);
            AddCommand = new AsyncRelayCommand(AddProduct);
            EditCommand = new AsyncRelayCommand(EditProduct);
            StopCommand = new RelayCommand(StopSellingProduct);

            foreach (var item in StatusFilters)
                item.PropertyChanged += OnStatusItemPropertyChanged;

            StatusFilters.CollectionChanged += OnStatusFiltersCollectionChanged;

            RequestPage = async () => await UpdateProductList();
            RequestPage?.Invoke();
        }

        private void OnStatusFiltersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (StatusItem item in e.NewItems)
                    item.PropertyChanged += OnStatusItemPropertyChanged;
            }

            if (e.OldItems != null)
            {
                foreach (StatusItem item in e.OldItems)
                    item.PropertyChanged -= OnStatusItemPropertyChanged;
            }
        }

        private void OnStatusItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_isUpdating) return;
            if (e.PropertyName != nameof(StatusItem.IsChecked)) return;

            if (StatusFilters.All(x => !x.IsChecked))
            {
                try
                {
                    _isUpdating = true;
                    StatusFilters[0].IsChecked = true;
                }
                finally
                {
                    _isUpdating = false;
                }
            }
        }

        public async void SearchProduct()
        {
            if (CurrentSearch == null) CurrentSearch = new();
            if (CurrentSearch.Statuses == null) CurrentSearch.Statuses = new List<ProductStatus>();
            if (CurrentSearch.Statuses.Any()) CurrentSearch.Statuses.Clear();

            CurrentSearch.NameOrCode = string.IsNullOrWhiteSpace(SearchInput)
                ? null
                : SearchInput.Trim();

            CurrentSearch.SelectedCategory = default;
            CurrentSearch.UsageType = ProductUsageType.All;
            CurrentSearch.MinUsageValue = null;
            CurrentSearch.MaxUsageValue = null;
            CurrentSearch.StartType = null;

            foreach (var status in StatusFilters.Where(status => status.IsChecked))
            {
                CurrentSearch?.Statuses?.Add(status.Status);
            }

            await UpdateProductList();
        }

        public void ResetFilters()
        {
            StatusFilters[0].IsChecked = true;
            StatusFilters[1].IsChecked = false;
            StatusFilters[2].IsChecked = false;

            SelectedUsageType = ProductUsageType.All;

            MinUsageValue = string.Empty;
            MaxUsageValue = string.Empty;

            MinPrice = string.Empty;
            MaxPrice = string.Empty;

            SelectedProductInfo = NO_INFO_TEXT;
            ComponentSummary = NO_INFO_TEXT;

            SearchProduct();
        }

        private async Task AddProduct()
        {
            var vm = App.Services.GetRequiredService<ProductEditorViewModel>();

            var win = new ProductEditorWindow
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };

            var ok = win.ShowDialog() == true;

            if (ok)
                await UpdateProductList();
        }

        private async Task EditProduct()
        {
            if (SelectedProduct == null) return;
            var vm = new ProductEditorViewModel(
                    App.Services.GetRequiredService<IProductService>(),
                    SelectedProduct);

            var win = new ProductEditorWindow
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };

            var ok = win.ShowDialog() == true;

            if (ok)
                await UpdateProductList();
        }

        private void StopSellingProduct()
        {
            if (SelectedProduct == null) return;
            SelectedProduct.Status = ProductStatus.Stopped;
            // TODO(ProductComponent): 상태 변경 저장 시 기존 구성품을 조회해 ProductUpsertRequest에 포함해야 한다.
            SearchProduct();
        }

        private async Task UpdateProductList()
        {
            CurrentSearch.SortBy = "created_at";
            CurrentSearch.Desc = false;

            var result = await _productService.SearchAsync(CurrentSearch);

            Items.Clear();
            foreach (var item in result) Items.Add(item);

            TotalCount = Math.Max(0, result.Count);
        }

        #region Helpers

        private async Task LoadComponentSummaryAsync(Product product)
        {
            try
            {
                var components = await _productService.GetComponentsAsync(product.Id);

                if (SelectedProduct?.Id != product.Id)
                    return;

                ComponentSummary = CreateComponentSummaryText(components);
            }
            catch (Exception ex)
            {
                if (SelectedProduct?.Id != product.Id)
                    return;

                ComponentSummary = NO_INFO_TEXT;
                MessageBox.Show(ex.Message);
            }
        }

        private string CreateProductInfoText(Product product)
        {
            string saleType = product.SaleType.GetEnumDescription();
            string status = product.Status.GetEnumDescription();

            return $"{product.Name} | {saleType} | {GiveUnitToPrice(product.Price)} | 상태: {status}";
        }

        private string CreateComponentSummaryText(IReadOnlyList<ProductComponent> components)
        {
            if (components.Count == 0)
                return NO_INFO_TEXT;

            return string.Join(" + ", components.Select(CreateComponentSummaryText));
        }

        private string CreateComponentSummaryText(ProductComponent component)
        {
            string category = GetComponentCategoryText(component.Category);
            string? usageValue = GiveUnitToUsageValue(component.UsageType, component.UsageValue);

            return string.IsNullOrWhiteSpace(usageValue)
                ? category
                : $"{category} {usageValue}";
        }

        private string GetComponentCategoryText(ProductCategory category)
        {
            return category switch
            {
                ProductCategory.Gym => "헬스",
                ProductCategory.PT => "PT",
                ProductCategory.Locker => "락커",
                ProductCategory.Wear => "운동복",
                ProductCategory.Etc => "기타",
                _ => category.GetEnumDescription()
            };
        }

        private string? GiveUnitToUsageValue(ProductUsageType usageType, int? usageValue)
        {
            string? valueText = usageValue?.ToString("N0");
            switch (usageType)
            {
                case ProductUsageType.Period:
                    valueText += "일";
                    break;
                case ProductUsageType.Count:
                    valueText += "회";
                    break;
            }
            return valueText;
        }

        private string GiveUnitToPrice(int price) => price.ToString("N0") + " ₩"; 

        #endregion
    }
}
