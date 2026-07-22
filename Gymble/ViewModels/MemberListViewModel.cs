using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gymble.Models;
using Gymble.Services;
using Gymble.ViewModels.Popup;
using Gymble.Views.Popup;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Gymble.ViewModels
{
    public partial class MemberListViewModel : PagingViewModel<Member>
    {
        public string PageTitle { get; set; } = "회원 관리";

        public MemberSearch CurrentSearch { get; private set; } = new();

        public ObservableCollection<Member> MemberList { get; } = new();
        public ObservableCollection<MemberMembershipSummary> CurrentMemberships { get; } = new();
        public ObservableCollection<MemberMembershipSummary> MembershipHistories { get; } = new();

        [ObservableProperty]
        private string searchInput = string.Empty;

        [ObservableProperty]
        private Member? selectedMember;

        [ObservableProperty]
        private bool isDrawerOpen;

        [ObservableProperty]
        private bool isMembershipLoading;

        [ObservableProperty]
        private string? membershipErrorMessage;

        partial void OnSelectedMemberChanged(Member? value)
        {
            IsDrawerOpen = value != null;
            _ = LoadMembershipsAsync(value?.Id);
        }

        public IAsyncRelayCommand? SearchCommand { get; }
        public IAsyncRelayCommand? AddCommand { get; }
        public IAsyncRelayCommand? EditCommand { get; }
        public IAsyncRelayCommand? DeleteCommand { get; }
        public ICommand? CloseInfoViewCommand { get; }
        public IAsyncRelayCommand? PurchaseProductCommand { get; }

        #region Fields

        private readonly IMemberService _memberService;
        private readonly IMembershipService _membershipService;

        #endregion

        public MemberListViewModel(IMemberService memberService, IMembershipService membershipService)
        {
            _memberService = memberService;
            _membershipService = membershipService;

            SearchCommand = new AsyncRelayCommand(SearchMember);
            AddCommand = new AsyncRelayCommand(AddMember);
            EditCommand = new AsyncRelayCommand(EditMember);
            DeleteCommand = new AsyncRelayCommand(DeleteMember);
            CloseInfoViewCommand = new RelayCommand(CloseInfoView);
            PurchaseProductCommand = new AsyncRelayCommand(PurchaseProduct);

            RequestPage = async () => await UpdateMemberList();
            RequestPage?.Invoke();
        }

        public async Task SearchMember()
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
            if (SelectedMember == null) return;

            var msgResult = MessageBox.Show("정말로 삭제하겠습니까?", "경고", MessageBoxButton.OKCancel);

            if (msgResult == MessageBoxResult.Cancel) return;

            await _memberService.DeleteAsync(SelectedMember);

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

        private async Task LoadMembershipsAsync(int? memberId)
        {
            CurrentMemberships.Clear();
            MembershipHistories.Clear();
            MembershipErrorMessage = null;

            if (!memberId.HasValue || memberId.Value <= 0)
                return;

            IsMembershipLoading = true;

            try
            {
                var memberships = await _membershipService.GetByMemberIdAsync(memberId.Value);

                if (SelectedMember?.Id != memberId.Value)
                    return;

                foreach (var membership in memberships)
                {
                    if (membership.Status == MembershipStatus.Active)
                        CurrentMemberships.Add(membership);

                    MembershipHistories.Add(membership);
                }
            }
            catch (Exception ex)
            {
                if (SelectedMember?.Id == memberId.Value)
                    MembershipErrorMessage = ex.Message;
            }
            finally
            {
                if (SelectedMember?.Id == memberId.Value)
                    IsMembershipLoading = false;
            }
        }

        private async Task PurchaseProduct()
        {
            if (SelectedMember == null) return;

            var vm = App.Services.GetRequiredService<PurchaseProductViewModel>();
            vm.Initialize(SelectedMember);

            var win = new PurchaseProductWindow
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };

            var ok = win.ShowDialog() == true;

            if (ok)
            {
                await UpdateMemberList();
                await LoadMembershipsAsync(SelectedMember?.Id);
            }
        }
    }
}
