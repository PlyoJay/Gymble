using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gymble.Models;
using Gymble.Services;
using Gymble.Utils;
using Gymble.ViewModels.Popup;
using Gymble.Views.Popup;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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

        public string UsageValueHeader => SelectedUsageType switch
        {
            ProductUsageType.Period => "이용량 (일)",
            ProductUsageType.Count => "이용량 (회)",
            _ => "이용량 (일/회)"
        };

        [ObservableProperty]
        private string searchInput = string.Empty;

        [ObservableProperty]
        private ProductCategory? selectedCategory;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(UsageValueHeader))]
        private ProductUsageType selectedUsageType = ProductUsageType.All;

        [ObservableProperty]
        private string minUsageValue = string.Empty;

        [ObservableProperty]
        private string maxUsageValue = string.Empty;

        [ObservableProperty]
        private string minPrice = string.Empty;

        [ObservableProperty]
        private string maxPrice = string.Empty;

        [ObservableProperty]
        private ProductStartType? selectedStartType;

        [ObservableProperty]
        private string usageValueError = string.Empty;

        [ObservableProperty]
        private string priceError = string.Empty;

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

        [ObservableProperty]
        private bool isSearching;

        partial void OnSelectedCategoryChanged(ProductCategory? value)
        {
            CurrentSearch.SelectedCategory = value;
            SearchAfterFilterChanged();
        }

        partial void OnSelectedUsageTypeChanged(ProductUsageType value)
        {
            CurrentSearch.UsageType = value;
            SearchAfterFilterChanged();
        }

        partial void OnMinUsageValueChanged(string value)
        {
            CurrentSearch.MinUsageValue = ParseOptionalInt(value);
            SearchAfterFilterChanged();
        }

        partial void OnMaxUsageValueChanged(string value)
        {
            CurrentSearch.MaxUsageValue = ParseOptionalInt(value);
            SearchAfterFilterChanged();
        }

        partial void OnMinPriceChanged(string value)
        {
            CurrentSearch.MinPrice = ParseOptionalInt(value);
            SearchAfterFilterChanged();
        }

        partial void OnMaxPriceChanged(string value)
        {
            CurrentSearch.MaxPrice = ParseOptionalInt(value);
            SearchAfterFilterChanged();
        }

        partial void OnSelectedStartTypeChanged(ProductStartType? value)
        {
            CurrentSearch.StartType = value;
            SearchAfterFilterChanged();
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

        public IAsyncRelayCommand? SearchCommand { get; }
        public ICommand? ResetFilterCommand { get; }
        public IAsyncRelayCommand? AddCommand { get; }
        public ICommand? EditCommand { get; }
        public ICommand? StopCommand { get; }
        public ICommand? DeleteCommand { get; }

        private readonly IProductService _productService;
        private CancellationTokenSource? _searchCts;
        private int _searchRequestVersion;

        public Action? RequestPage { get; set; }

        private const string NO_INFO_TEXT = "없음";

        private bool _isUpdating;
        private bool _suppressSearch;

        public ProductViewModel(IProductService productService)
        {
            _productService = productService;

            SearchCommand = new AsyncRelayCommand(SearchProduct);
            ResetFilterCommand = new RelayCommand(ResetFilters);
            AddCommand = new AsyncRelayCommand(AddProduct);
            EditCommand = new AsyncRelayCommand(EditProduct);
            StopCommand = new RelayCommand(StopSellingProduct);

            foreach (var item in StatusFilters)
                item.PropertyChanged += OnStatusItemPropertyChanged;

            StatusFilters.CollectionChanged += OnStatusFiltersCollectionChanged;

            RequestPage = async () => await SearchProduct();
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

            EnsureAtLeastOneStatusSelected();
        }

        public async Task SearchProduct()
        {
            if (!TryBuildSearch(out var search))
            {
                CancelPendingSearch();
                return;
            }

            await UpdateProductList(search);
        }

        public void ResetFilters()
        {
            try
            {
                _suppressSearch = true;
                _isUpdating = true;

                StatusFilters[0].IsChecked = true;
                StatusFilters[1].IsChecked = false;
                StatusFilters[2].IsChecked = false;

                CurrentSearch = new ProductSearch();

                SearchInput = string.Empty;
                SelectedCategory = null;
                SelectedUsageType = ProductUsageType.All;
                SelectedStartType = null;

                MinUsageValue = string.Empty;
                MaxUsageValue = string.Empty;

                MinPrice = string.Empty;
                MaxPrice = string.Empty;

                UsageValueError = string.Empty;
                PriceError = string.Empty;

                SelectedProductInfo = NO_INFO_TEXT;
                ComponentSummary = NO_INFO_TEXT;
            }
            finally
            {
                _isUpdating = false;
                _suppressSearch = false;
            }

            _ = SearchProduct();
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
                await SearchProduct();
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
                await SearchProduct();
        }

        private void StopSellingProduct()
        {
            if (SelectedProduct == null) return;
            SelectedProduct.Status = ProductStatus.Stopped;
            // TODO(ProductComponent): 상태 변경 저장 시 기존 구성품을 조회해 ProductUpsertRequest에 포함해야 한다.
            _ = SearchProduct();
        }

        private async Task UpdateProductList(ProductSearch search)
        {
            var requestVersion = Interlocked.Increment(ref _searchRequestVersion);
            var previousCts = _searchCts;
            previousCts?.Cancel();

            using var cts = new CancellationTokenSource();
            _searchCts = cts;
            IsSearching = true;

            try
            {
                var result = await _productService.SearchAsync(search, cts.Token);

                if (cts.IsCancellationRequested || requestVersion != _searchRequestVersion)
                    return;

                Items.Clear();
                foreach (var item in result) Items.Add(item);

                TotalCount = Math.Max(0, result.Count);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                if (requestVersion == _searchRequestVersion)
                    MessageBox.Show(ex.Message);
            }
            finally
            {
                if (ReferenceEquals(_searchCts, cts))
                    _searchCts = null;

                if (requestVersion == _searchRequestVersion)
                    IsSearching = false;
            }
        }

        private void SearchAfterFilterChanged()
        {
            if (_suppressSearch)
                return;

            _ = SearchProduct();
        }

        private bool TryBuildSearch(out ProductSearch search)
        {
            EnsureAtLeastOneStatusSelected();

            var minUsageValue = ParseOptionalInt(MinUsageValue);
            var maxUsageValue = ParseOptionalInt(MaxUsageValue);
            var minPrice = ParseOptionalInt(MinPrice);
            var maxPrice = ParseOptionalInt(MaxPrice);

            CurrentSearch.NameOrCode = string.IsNullOrWhiteSpace(SearchInput)
                ? null
                : SearchInput.Trim();

            CurrentSearch.SelectedCategory = SelectedCategory;
            CurrentSearch.UsageType = SelectedUsageType;
            CurrentSearch.MinUsageValue = minUsageValue;
            CurrentSearch.MaxUsageValue = maxUsageValue;
            CurrentSearch.MinPrice = minPrice;
            CurrentSearch.MaxPrice = maxPrice;
            CurrentSearch.StartType = SelectedStartType;
            CurrentSearch.SortBy = "created_at";
            CurrentSearch.Desc = false;
            CurrentSearch.Statuses ??= new List<ProductStatus>();
            CurrentSearch.Statuses.Clear();

            foreach (var status in StatusFilters.Where(status => status.IsChecked))
                CurrentSearch.Statuses.Add(status.Status);

            UsageValueError = string.Empty;
            PriceError = string.Empty;

            var hasError = false;

            if (minUsageValue.HasValue && maxUsageValue.HasValue && minUsageValue.Value > maxUsageValue.Value)
            {
                UsageValueError = "최소 이용량은 최대 이용량보다 클 수 없습니다.";
                hasError = true;
            }

            if (minPrice.HasValue && maxPrice.HasValue && minPrice.Value > maxPrice.Value)
            {
                PriceError = "최소 가격은 최대 가격보다 클 수 없습니다.";
                hasError = true;
            }

            if (hasError)
            {
                search = CreateSearchSnapshot(CurrentSearch);
                return false;
            }

            search = CreateSearchSnapshot(CurrentSearch);
            return true;
        }

        private void EnsureAtLeastOneStatusSelected()
        {
            if (StatusFilters.Any(x => x.IsChecked))
                return;

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

        private void CancelPendingSearch()
        {
            _searchCts?.Cancel();
        }

        private static int? ParseOptionalInt(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return int.TryParse(value, out var parsed) ? parsed : null;
        }

        private static ProductSearch CreateSearchSnapshot(ProductSearch source)
        {
            return new ProductSearch
            {
                NameOrCode = source.NameOrCode,
                SaleType = source.SaleType,
                SelectedCategory = source.SelectedCategory,
                Statuses = source.Statuses?.ToList(),
                UsageType = source.UsageType,
                MinUsageValue = source.MinUsageValue,
                MaxUsageValue = source.MaxUsageValue,
                MinPrice = source.MinPrice,
                MaxPrice = source.MaxPrice,
                IsFavorite = source.IsFavorite,
                StartType = source.StartType,
                SortBy = source.SortBy,
                Desc = source.Desc,
                Take = source.Take,
                Skip = source.Skip
            };
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

        private string GiveUnitToPrice(int price) => price.ToString("N0") + "원";

        #endregion
    }
}
