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

public enum ProductEditorMode
{
    Add,
    Edit
}

namespace Gymble.ViewModels.Popup
{
    public partial class ProductEditorViewModel : ObservableObject
    {
        public IEnumerable<ProductCategory> Categories
            => Enum.GetValues(typeof(ProductCategory)).Cast<ProductCategory>();

        public IEnumerable<ProductStartType> StartTypes
            => Enum.GetValues(typeof(ProductStartType)).Cast<ProductStartType>();

        public bool IsAddMode => _mode == ProductEditorMode.Add;
        public bool IsEditMode => _mode == ProductEditorMode.Edit;

        public string Title => IsAddMode ? "상품 등록" : "상품 수정";
        public string SaveButtonText => IsAddMode ? "추가" : "저장";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private string productName = string.Empty;

        [ObservableProperty]
        private bool isAutoCode = false;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private string productCode = string.Empty;

        [ObservableProperty]
        private ProductCategory selectedCategory = ProductCategory.Gym;

        [ObservableProperty]
        private ProductUsageType selectedUsageType = ProductUsageType.Period;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private int usageValue = 0;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
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
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
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
        public IAsyncRelayCommand? SaveCommand { get; }

        #region Fields

        private readonly IProductService _productService;
        private readonly ProductEditorMode _mode;
        private readonly Product? _originalProduct;

        public bool IsCodeReadOnly => IsAutoCode;

        #endregion

        #region Events

        public event Action<bool>? RequestClose;

        #endregion

        public ProductEditorViewModel(IProductService productService)
        {
            _productService = productService;
            _mode = ProductEditorMode.Add;

            CloseCommand = new RelayCommand(() => Close(false));
            SaveCommand = new AsyncRelayCommand(SaveProductAsync, CanSave);
        }

        public ProductEditorViewModel(IProductService productService, Product product)
        {
            _productService = productService;
            _mode = ProductEditorMode.Edit;
            _originalProduct = product;

            CloseCommand = new RelayCommand(() => Close(false));
            SaveCommand = new AsyncRelayCommand(SaveProductAsync, CanSave);

            LoadFrom(product);
        }

        private void LoadFrom(Product product)
        {
            ProductName = product.Name ?? string.Empty;
            ProductCode = product.Code ?? string.Empty;
            SelectedCategory = product.Category;
            SelectedUsageType = product.UsageType;
            UsageValue = (int)product.UsageValue;
            Price = product.Price;
            SelectedStartType = product.StartType;
            FixedDateTime = product.FixedStartDate;
            SelectedStatus = product.Status;
            IsFavorite = product.IsFavorite;
            Note = product.Note;

            IsAutoCode = false;
        }

        private async Task SaveProductAsync()
        {
            if (!CanSave())
            {
                MessageBox.Show("필수 입력값을 확인해주세요.");
                return;
            }

            bool isSuccess = false;

            try
            {
                IsBusy = true;

                if (IsAddMode)
                {
                    var newProduct = new Product
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
                        IsFavorite = IsFavorite,
                        Note = Note
                    };

                    await _productService.AddAsync(newProduct);
                }
                else
                {
                    if (_originalProduct == null)
                        throw new InvalidOperationException("수정할 상품 정보가 없습니다.");

                    _originalProduct.Name = ProductName;
                    _originalProduct.Code = ProductCode;
                    _originalProduct.Category = SelectedCategory;
                    _originalProduct.UsageType = SelectedUsageType;
                    _originalProduct.UsageValue = UsageValue;
                    _originalProduct.Price = Price;
                    _originalProduct.StartType = SelectedStartType;
                    _originalProduct.FixedStartDate = FixedDateTime;
                    _originalProduct.Status = SelectedStatus;
                    _originalProduct.IsFavorite = IsFavorite;
                    _originalProduct.Note = Note;

                    await _productService.UpdateAsync(_originalProduct);
                }

                isSuccess = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                IsBusy = false;

                if (isSuccess)
                    Close(true);
            }
        }

        private void Close(bool result)
        {
            if (IsBusy) return;
            RequestClose?.Invoke(result);
        }

        #region Helpers

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(ProductName)
                && (IsAutoCode || !string.IsNullOrWhiteSpace(ProductCode))
                && UsageValue > 0
                && Price > 0
                && (SelectedStartType != ProductStartType.FixedDate || FixedDateTime.HasValue)
                && !IsBusy;
        }

        #endregion 
    }
}
