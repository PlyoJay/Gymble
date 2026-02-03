using Gymble.Controls;
using Gymble.Models;
using Gymble.Services;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Gymble.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Properties

        public ObservableCollection<NavigationItemModel> NavigationItems { get; }
        public ObservableCollection<NavigationItemModel> SettingsItem { get; }

        private NavigationItemModel _selectedNavItem;
        public NavigationItemModel SelectedNavItem
        {
            get => _selectedNavItem;
            set
            {
                _selectedNavItem = value;
                OnPropertyChanged();

                if (_selectedNavItem?.Command?.CanExecute(_selectedNavItem.TagName) == true)
                    _selectedNavItem.Command.Execute(_selectedNavItem.TagName);
            }
        }

        private NavigationItemModel _selectedSettings;
        public NavigationItemModel SelectedSettings
        {
            get => _selectedSettings;
            set
            {
                _selectedSettings = value;
                OnPropertyChanged();

                if (_selectedSettings?.Command?.CanExecute(_selectedSettings.TagName) == true)
                    _selectedSettings.Command.Execute(_selectedSettings.TagName);
            }
        }

        private object? _currentViewModel;
        public object? CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                Console.WriteLine($"CurrentViewModel changed to: {_currentViewModel?.GetType().Name}");
                OnPropertyChanged();
            }
        }

        private bool _isDrawerOpen;
        public bool IsDrawerOpen
        {
            get => _isDrawerOpen;
            set
            {
                if (_isDrawerOpen != value)
                {
                    _isDrawerOpen = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Command

        public ICommand CloseCommand { get; }
        public ICommand ToDashboardViewCommand { get; }
        public ICommand ToMemberLIstViewCommand { get; }
        public ICommand ToAttendaceViewCommand { get; }
        public ICommand ToProductViewCommand { get; }
        public ICommand ToMembershipViewCommand { get; }
        public ICommand ToSettingsViewCommand { get; }

        #endregion

        #region Fields

        private readonly IServiceProvider _sp;

        #endregion

        public MainWindowViewModel(IServiceProvider sp)
        {
            _sp = sp;

            CloseCommand = new RelayCommand<Window>(w => Close(w), w => w != null);
            ToDashboardViewCommand = new RelayCommand(SwapView);
            ToMemberLIstViewCommand = new RelayCommand(SwapView);
            ToAttendaceViewCommand = new RelayCommand(SwapView);
            ToProductViewCommand = new RelayCommand(SwapView);
            ToSettingsViewCommand = new RelayCommand(SwapView);

            CurrentViewModel = new DashboardViewModel();

            NavigationItems = new ObservableCollection<NavigationItemModel>()
            {
                new NavigationItemModel { IconKind = PackIconKind.MonitorDashboard, Label = "대시보드", TagName="dashboard", Command = ToDashboardViewCommand },
                new NavigationItemModel { IconKind = PackIconKind.ViewList, Label = "회원관리", TagName="memberlist", Command = ToMemberLIstViewCommand },
                new NavigationItemModel { IconKind = PackIconKind.ListStatus, Label = "출석관리", TagName="attendance", Command = ToAttendaceViewCommand },
                new NavigationItemModel { IconKind = PackIconKind.Gym, Label = "상품관리", TagName="product", Command = ToProductViewCommand },
                //new NavigationItemModel { IconKind = PackIconKind.CardMembership, Label = "멤버쉽", TagName="membership", Command = ToMembershipViewCommand },                
            };

            SettingsItem = new ObservableCollection<NavigationItemModel>()
            {
                new NavigationItemModel { IconKind = PackIconKind.Settings, Label = "설정", TagName="settings", Command = ToSettingsViewCommand }
            };

            SQLiteManager.Instance.GetAllRepositories();
        }

        private void Close(Window w)
        {
            //w.DialogResult = result;
            w.Close();
        }

        private void SwapView(object obj)
        {
            string? tag = obj as string;

            switch (tag)
            {
                case "dashboard":
                    CurrentViewModel = _sp.GetRequiredService<DashboardViewModel>();
                    break;
                case "memberlist":
                    CurrentViewModel = _sp.GetRequiredService<MemberListViewModel>();
                    break;
                case "attendance":
                    CurrentViewModel = _sp.GetRequiredService<AttendanceViewModel>();
                    break;
                case "product":
                    CurrentViewModel = _sp.GetRequiredService<ProductViewModel>();
                    break;
                case "settings":
                    CurrentViewModel = _sp.GetRequiredService<SettingsViewModel>();
                    break;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
