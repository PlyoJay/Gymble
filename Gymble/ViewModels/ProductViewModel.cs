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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Gymble.ViewModels
{
    public class StatusItem
    {
        public ProductStatus Status { get; set; }
        public string Name { get; set; }
        public bool IsChecked { get; set; }
    }

    public partial class ProductViewModel : ObservableObject
    {
        public string PageTitle { get; set; } = "상품 관리";

        public ObservableCollection<Product> Items { get; } = new();

        public ProductSearch CurrnetSearch { get; private set; } = new();

        public ObservableCollection<StatusItem> StatusFilters { get; } =
        [
            new StatusItem { Status = ProductStatus.OnSale, Name="판매중"},
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
        private Product selectedProduct;

        [ObservableProperty]
        private int totalCount;

        partial void OnSelectedCategoryChanged(ProductCategory value)
        {
            CurrnetSearch.SelectedCategory = value;
            RequestPage?.Invoke();
        }

        public ICommand? AddFilterCommand { get; }
        public ICommand? SearchCommand { get; }
        public ICommand? AddCommand { get; }
        public ICommand? EditCommand { get; }
        public ICommand? StopCommand { get; }
        public ICommand? DeleteCommand { get; }

        #region Fields

        private readonly IProductService _productService;

        #endregion

        public Action? RequestPage { get; set; }

        public ProductViewModel(IProductService productService)
        {
            _productService = productService;

            SearchCommand = new RelayCommand(SearchProduct);

            AddCommand = new RelayCommand(AddProduct);

            RequestPage = async () => await UpdateProductList();
            RequestPage?.Invoke();
        }

        public async void SearchProduct()
        {
            if (CurrnetSearch == null) CurrnetSearch = new();
            if (CurrnetSearch.Statuses == null) CurrnetSearch.Statuses = new List<ProductStatus>();
            if (CurrnetSearch.Statuses.Any()) CurrnetSearch.Statuses.Clear();

            CurrnetSearch.SelectedCategory = ProductCategory.Gym;

            foreach (var status in StatusFilters.Where(status => status.IsChecked))
            {
                CurrnetSearch?.Statuses?.Add(status.Status);
            }

            CurrnetSearch.UsageType = SelectedUsageType;
            await UpdateProductList();
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
            CurrnetSearch.SortBy = "Id";
            CurrnetSearch.Desc = false;

            var result = await _productService.SearchAsync(CurrnetSearch);

            Items.Clear();
            foreach (var item in result) Items.Add(item);

            TotalCount = Math.Max(0, result.Count);
        }
    }
}
