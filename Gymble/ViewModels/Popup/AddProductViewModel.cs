using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private string productName = string.Empty;

        private ProductCategory _selectedCategory;
        public ProductCategory SelectedCategory
        {
            get => _selectedCategory;
            set => SetProperty(ref _selectedCategory, value);
        }

        [ObservableProperty]
        private ProductUsageType selectedUsageType = ProductUsageType.Period;

        [ObservableProperty]
        private int usageValue = 0;

        [ObservableProperty]
        private int price = 0;

        private ProductStartType _selectedStartType;
        public ProductStartType SlectedStartType
        {
            get => _selectedStartType;
            set => SetProperty(ref _selectedStartType, value);
        }

        [ObservableProperty]
        private ProductStatus selectedStatus = ProductStatus.OnSale;

        [ObservableProperty]
        private string? note;

        public ICommand? CloseCommand { get; }
        public ICommand? AddCommand { get; }

        #region Events

        public event Action<bool>? RequestClose;

        #endregion

        public AddProductViewModel()
        {
            
        }
    }
}
