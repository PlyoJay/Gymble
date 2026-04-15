using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gymble.Models;
using Gymble.Services;
using Gymble.ViewModels.Popup;
using Gymble.Views;
using Gymble.Views.Popup;
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
    public partial class MemberListViewModel : PagingViewModel<Member>
    {
        public string PageTitle { get; set; } = "회원 관리";

        public MemberSearch CurrentSearch { get; private set; } = new();

        public ObservableCollection<Member> MemberList { get; } = new();

        [ObservableProperty]
        private string searchInput = string.Empty;

        [ObservableProperty]
        private Member? selectedMember;

        [ObservableProperty]
        private bool isDrawerOpen;

        partial void OnSelectedMemberChanged(Member value)
        {
            IsDrawerOpen = true;
        }

        public ICommand? SearchCommand { get; }
        public IAsyncRelayCommand? AddCommand { get; }
        public IAsyncRelayCommand? EditCommand { get; }
        public IAsyncRelayCommand? DeleteCommand { get; }
        public ICommand? CloseInfoViewCommand { get; }
        public ICommand? PurchaseProductCommand { get; }

        #region Fields

        private readonly IMemberService _memberService;

        #endregion

        public MemberListViewModel(IMemberService memberService)
        {
            _memberService = memberService;

            SearchCommand = new RelayCommand(SearchMember);
            AddCommand = new AsyncRelayCommand(AddMember);
            EditCommand = new AsyncRelayCommand(EditMember);
            DeleteCommand = new AsyncRelayCommand(DeleteMember);
            CloseInfoViewCommand = new RelayCommand(CloseInfoView);
            PurchaseProductCommand = new RelayCommand(PurchaseProduct);

            RequestPage = async () => await UpdateMemberList();
            RequestPage?.Invoke();
        }

        public async void SearchMember()
        {
            if (CurrentSearch == null) CurrentSearch = new();

            CurrentSearch.NameOrPhone = SearchInput;

            await UpdateMemberList();
        }

        private async Task AddMember()
        {
            var vm = App.Services.GetRequiredService<AddMemberViewModel>();

            var win = new AddMemberWindow
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };

            var ok = win.ShowDialog() == true;

            if (ok)
                await UpdateMemberList();
        }

        private async Task EditMember()
        {
            if (SelectedMember == null) return;

            var vm = App.Services.GetRequiredService<EditMemberViewModel>();
            vm.Initialize(SelectedMember);

            var win = new EditMemberWindow 
            { 
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };

            var ok = win.ShowDialog() == true;

            if (ok)
                await UpdateMemberList();
        }

        private async Task DeleteMember()
        {
            var msgResult = MessageBox.Show("정말로 삭제하겠습니까?", "경고", MessageBoxButton.OKCancel);

            if (msgResult == MessageBoxResult.Cancel) return;

            await  _memberService.DeleteAsync(SelectedMember);

            SelectedMember = null;
            IsDrawerOpen = false;

            await UpdateMemberList();            
        }


        public async Task InitializeAsync()
        {
            await UpdateMemberList();
        }

        private async Task UpdateMemberList()
        {
            CurrentSearch.Page = PageIndex + 1;   // 0-based → 1-based
            CurrentSearch.PageSize = PageSize;
            CurrentSearch.SortBy = "id";
            CurrentSearch.Desc = false;

            var result = await _memberService.SearchAsync(CurrentSearch);

            ApplyPage(result.Rows, result.Total, result.Page);
        }

        private void CloseInfoView()
        {
            SelectedMember = null;
            IsDrawerOpen = false;
        }

        private void PurchaseProduct()
        {
            var vm = App.Services.GetRequiredService<PurchaseProductViewModel>();
            vm.Initialize(SelectedMember);

            var win = new PurchaseProductWindow
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };

            var ok = win.ShowDialog() == true;
        }
    }
}
