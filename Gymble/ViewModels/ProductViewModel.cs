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
    public class ProductViewModel : ObservableObject
    {
        public string PageTitle { get; set; } = "상품 관리";

        public ProductSearch CurrnetSearch { get; private set; } = new();

        public ObservableCollection<Product> Items { get; } = new();

        public ICommand? SearchCommand { get; }
        public ICommand? AddCommand { get; }
        public ICommand? EditCommand { get; }
        public ICommand? StopCommand { get; }
        public ICommand? DeleteCommand { get; }

        #region Fields

        private readonly IProductService _productService;

        #endregion

        public ProductViewModel(IProductService productService)
        {
            _productService = productService;

            AddCommand = new RelayCommand(AddProduct);
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
    }
}
