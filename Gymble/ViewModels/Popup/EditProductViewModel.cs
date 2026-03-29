using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gymble.Models;
using Gymble.Repositories;
using Gymble.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Gymble.ViewModels.Popup
{
    public partial class EditProductViewModel : ObservableObject
    {
        [ObservableProperty]
        private Product targetProduct;

        public IEnumerable<ProductCategory> Categories
            => Enum.GetValues(typeof(ProductCategory)).Cast<ProductCategory>();

        public IEnumerable<ProductStartType> StartTypes
            => Enum.GetValues(typeof(ProductStartType)).Cast<ProductStartType>();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(EditCommand))]
        private string productName = string.Empty;

        [ObservableProperty]
        private ProductCategory selectedCategory = ProductCategory.Gym;

        [ObservableProperty]
        private ProductUsageType selectedUsageType = ProductUsageType.Period;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(EditCommand))]
        private int usageValue = 0;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(EditCommand))]
        private int price = 0;

        [ObservableProperty]
        private ProductStartType selectedStartType = ProductStartType.Immediate;

        [ObservableProperty]
        private DateTime? fixedDateTime;

        [ObservableProperty]
        private ProductStatus selectedStatus = ProductStatus.OnSale;

        [ObservableProperty]
        private bool isFavorite;

        [ObservableProperty]
        private string? note;

        public ICommand? CloseCommand { get; }
        public IAsyncRelayCommand? EditCommand { get; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(EditCommand))]
        private bool isBusy = false;

        private readonly IProductService _productService;

        #region Events

        public event Action<bool>? RequestClose;

        #endregion

        public EditProductViewModel(IProductService productService)
        {
            _productService = productService;

            CloseCommand = new RelayCommand(() => Close(false));
        }

        public void SetProduct(Product product)
        {
            TargetProduct = product;

            ProductName = TargetProduct.Name;
        }

        private void Close(bool result)
        {
            if (IsBusy) return;
            RequestClose?.Invoke(result);
        }
    }
}
