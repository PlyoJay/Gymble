using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gymble.Models;
using Gymble.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Gymble.ViewModels.Popup
{
    public partial class AddProductViewModel : ObservableObject
    {
        public IEnumerable<ProductCategory> Categories
            => Enum.GetValues(typeof(ProductCategory)).Cast<ProductCategory>();

        public IEnumerable<ProductStartType> StartTypes
            => Enum.GetValues(typeof(ProductStartType)).Cast<ProductStartType>();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        private string productName = string.Empty;

        [ObservableProperty]
        private bool isAutoCode = false;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        private string productCode = string.Empty;

        [ObservableProperty]
        private ProductCategory selectedCategory = ProductCategory.Gym;

        [ObservableProperty]
        private ProductUsageType selectedUsageType = ProductUsageType.Period;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        private int usageValue = 0;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
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

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        private bool isBusy = false;

        #region OnPropertyChanged

        partial void OnIsAutoCodeChanged(bool value)
        {
            OnPropertyChanged(nameof(IsCodeReadOnly));

            if (value)
                ProductCode = string.Empty;
        }

        #endregion

        public ICommand? CloseCommand { get; }
        public IAsyncRelayCommand? AddCommand { get; }

        #region Fields

        private readonly IProductService _productService;
        private readonly IProductCodeGenerator _codeGenerator;

        public bool IsCodeReadOnly => IsAutoCode;

        #endregion

        #region Events

        public event Action<bool>? RequestClose;

        #endregion

        public AddProductViewModel(IProductService productService, IProductCodeGenerator codeGenerator)
        {
            _productService = productService;
            _codeGenerator = codeGenerator;

            CloseCommand = new RelayCommand(() => Close(false));
            AddCommand = new AsyncRelayCommand(AddProductAsync, CanAdd);
        }

        private async Task AddProductAsync()
        {
            if (!CanAdd())
            {
                MessageBox.Show("필수 입력값을 확인해주세요.");
                return;
            }

            try
            {
                IsBusy = true;

                var product = new Product()
                {
                    Name = ProductName,
                    Code = IsAutoCode ? "" : ProductCode,
                    Category = SelectedCategory,
                    UsageType = SelectedUsageType,
                    UsageValue = UsageValue,
                    Price = Price,
                    StartType = SelectedStartType,
                    FixedStartDate = FixedDateTime,
                    Status = SelectedStatus,
                    IsFavorite = IsFavorite
                };

                await _productService.AddAsync(product);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                IsBusy = false;
                Close(true);
            }
        }

        private void Close(bool result)
        {
            if (IsBusy) return;
            RequestClose?.Invoke(result);
        }

        #region Helpers

        private bool CanAdd()
        {
            return !string.IsNullOrWhiteSpace(ProductName)
                && (IsAutoCode || !string.IsNullOrWhiteSpace(ProductCode))
                && UsageValue > 0
                && Price > 0
                && !IsBusy;
        }

        #endregion 
    }
}
