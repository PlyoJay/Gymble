using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gymble.Models;
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

        partial void OnSelectedCategoryChanged(ProductCategory value)
        {
            switch (value)
            {
                default:
                case ProductCategory.Gym:
                    break;
                case ProductCategory.PT:
                    break;
                case ProductCategory.Locker:
                    break;
                case ProductCategory.Wear:
                    break;
                case ProductCategory.Etc:
                    break;
            }
        }

        #endregion

        public ICommand? CloseCommand { get; }
        public IAsyncRelayCommand? AddCommand { get; }

        #region Events

        public event Action<bool>? RequestClose;

        #endregion

        public AddProductViewModel()
        {
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
                    Category = SelectedCategory,
                    UsageType = SelectedUsageType,
                    Price = Price,
                    StartType = SelectedStartType,
                    FixedStartDate = FixedDateTime,
                    Status = SelectedStatus,
                    IsFavorite = IsFavorite
                };
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
            return !string.IsNullOrEmpty(ProductName)
                && UsageValue >= 0
                && Price >= 0;
        }

        #endregion 
    }
}
