using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gymble.Models;
using Gymble.Services;
using System.Collections.ObjectModel;

namespace Gymble.ViewModels
{
    public partial class MembershipViewModel : ObservableObject
    {
        private readonly IMemberService _memberService;
        private readonly IMembershipService _membershipService;

        public string PageTitle { get; set; } = "이용권 조회";
        public ObservableCollection<Member> Members { get; } = new();
        public ObservableCollection<MemberMembershipSummary> Memberships { get; } = new();

        [ObservableProperty]
        private string searchInput = string.Empty;

        [ObservableProperty]
        private Member? selectedMember;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string? errorMessage;

        public IAsyncRelayCommand SearchCommand { get; }

        public MembershipViewModel(IMemberService memberService, IMembershipService membershipService)
        {
            _memberService = memberService ?? throw new ArgumentNullException(nameof(memberService));
            _membershipService = membershipService ?? throw new ArgumentNullException(nameof(membershipService));
            SearchCommand = new AsyncRelayCommand(SearchAsync);

            _ = SearchAsync();
        }

        partial void OnSelectedMemberChanged(Member? value)
        {
            _ = LoadMembershipsAsync(value?.Id);
        }

        private async Task SearchAsync()
        {
            IsBusy = true;
            ErrorMessage = null;

            try
            {
                var result = await _memberService.SearchAsync(new MemberSearch
                {
                    NameOrPhone = string.IsNullOrWhiteSpace(SearchInput) ? null : SearchInput.Trim(),
                    Page = 1,
                    PageSize = 100,
                    SortBy = "id",
                    Desc = false
                });

                Members.Clear();
                foreach (var member in result.Rows)
                    Members.Add(member);

                SelectedMember = Members.FirstOrDefault();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadMembershipsAsync(int? memberId)
        {
            Memberships.Clear();
            ErrorMessage = null;

            if (!memberId.HasValue || memberId.Value <= 0)
                return;

            IsBusy = true;

            try
            {
                var memberships = await _membershipService.GetByMemberIdAsync(memberId.Value);

                if (SelectedMember?.Id != memberId.Value)
                    return;

                foreach (var membership in memberships)
                    Memberships.Add(membership);
            }
            catch (Exception ex)
            {
                if (SelectedMember?.Id == memberId.Value)
                    ErrorMessage = ex.Message;
            }
            finally
            {
                if (SelectedMember?.Id == memberId.Value)
                    IsBusy = false;
            }
        }
    }
}
