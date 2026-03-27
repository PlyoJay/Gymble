using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gymble.Models;
using Gymble.Services;
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
        public string Name { get; set; }

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
            new StatusItem { Status = ProductStatus.OnSale, Name="판매중", IsChecked=true},
            new StatusItem { Status = ProductStatus.Stopped, Name="중지"},
            new StatusItem { Status = ProductStatus.Discontinued, Name="단종"}
        ];

        public IEnumerable<ProductUsageType> UsageTypes
            => Enum.GetValues(typeof(ProductUsageType)).Cast<ProductUsageType>();

        [ObservableProperty]
        private ProductCategory selectedCategory = ProductCategory.Gym;

        [ObservableProperty]
        private ProductUsageType selectedUsageType;

        [ObservableProperty]
        private string minUsageValue;

        [ObservableProperty]
        private string maxUsageValue;

        [ObservableProperty]
        private string minPrice;

        [ObservableProperty]
        private string maxPrice;

        [ObservableProperty]
        private Product selectedProduct;

        [ObservableProperty]
        private int totalCount;

        private const int MinFixedUsageValue = 0;
        private const int MaxFixedUsageValue = 9999;

        partial void OnSelectedCategoryChanged(ProductCategory value)
        {
            CurrentSearch.SelectedCategory = value;
            SearchProduct();
        }

        partial void OnSelectedUsageTypeChanged(ProductUsageType value)
        {
            CurrentSearch.UsageType = value;
            SearchProduct();
        }

        partial void OnMinUsageValueChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                CurrentSearch.MinUsageValue = null;
                SearchProduct();
                return;
            }

            if (!int.TryParse(value, out int minVal))
                return;

            if (minVal < MinFixedUsageValue)
                return;

            if (int.TryParse(MaxUsageValue, out int maxVal) && minVal > maxVal)
                return;

            CurrentSearch.MinUsageValue = minVal;
            SearchProduct();
        }

        partial void OnMaxUsageValueChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                CurrentSearch.MaxUsageValue = null;
                SearchProduct();
                return;
            }

            if (!int.TryParse(value, out int maxVal))
                return;

            if (maxVal > MaxFixedUsageValue)
                return;

            if (int.TryParse(MinUsageValue, out int minVal) && maxVal < minVal)
                return;

            CurrentSearch.MaxUsageValue = maxVal;
            SearchProduct();
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

        public ICommand? SearchCommand { get; }
        public ICommand? ResetFilterCommand { get; }
        public ICommand? AddCommand { get; }
        public ICommand? EditCommand { get; }
        public ICommand? StopCommand { get; }
        public ICommand? DeleteCommand { get; }

        #region Fields

        private readonly IProductService _productService;

        #endregion

        public Action? RequestPage { get; set; }

        private bool _isUpdating;

        public ProductViewModel(IProductService productService)
        {
            _productService = productService;

            SearchCommand = new RelayCommand(SearchProduct);
            ResetFilterCommand = new RelayCommand(ResetFilters);

            AddCommand = new RelayCommand(AddProduct);

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
        }

        private async void AddProduct()
        {
            var vm = App.Services.GetRequiredService<AddProductViewModel>();

            var win = new AddProductWindow
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };

            var ok = win.ShowDialog() == true;

            if (ok)
                await UpdateProductList();
        }

        private async Task UpdateProductList()
        {
            CurrentSearch.SortBy = "Id";
            CurrentSearch.Desc = false;

            var result = await _productService.SearchAsync(CurrentSearch);

            Items.Clear();
            foreach (var item in result) Items.Add(item);

            TotalCount = Math.Max(0, result.Count);
        }
    }
}
