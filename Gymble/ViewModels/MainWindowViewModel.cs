using Gymble.Controls;
using Gymble.Models;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Gymble.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<NavigationItemModel> NavigationItems { get; }

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

        public ICommand ToDashboardViewCommand { get; }
        public ICommand ToMemberLIstViewCommand { get; }
        public ICommand ToAttendaceViewCommand { get; }
        public ICommand ToProductViewCommand { get; }
        public ICommand ToMembershipViewCommand { get; }
        public ICommand ToSettingsViewCommand { get; }

        public MainWindowViewModel()
        {
            ToDashboardViewCommand = new RelayCommand(SwapView);
            ToMemberLIstViewCommand = new RelayCommand(SwapView);
            ToAttendaceViewCommand = new RelayCommand(SwapView);
            ToProductViewCommand = new RelayCommand(SwapView);
            ToMembershipViewCommand = new RelayCommand(SwapView);
            ToSettingsViewCommand = new RelayCommand(SwapView);

            NavigationItems = new ObservableCollection<NavigationItemModel>()
            {
                new NavigationItemModel { IconKind = PackIconKind.MonitorDashboard, Label = "대쉬보드", TagName="dashboard", Command = ToDashboardViewCommand },
                new NavigationItemModel { IconKind = PackIconKind.ViewList, Label = "회원 목록", TagName="memberlist", Command = ToMemberLIstViewCommand },
                new NavigationItemModel { IconKind = PackIconKind.ListStatus, Label = "출석 목록", TagName="attendace", Command = ToAttendaceViewCommand },
                new NavigationItemModel { IconKind = PackIconKind.Gym, Label = "상품", TagName="product", Command = ToProductViewCommand },
                new NavigationItemModel { IconKind = PackIconKind.CardMembership, Label = "멤버쉽", TagName="membership", Command = ToMembershipViewCommand },
                new NavigationItemModel { IconKind = PackIconKind.Settings, Label = "설정", TagName="settings", Command = ToSettingsViewCommand }
            };
        }

        private void SwapView(object obj)
        {
            string tag = obj as string;
            Console.WriteLine(tag);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
