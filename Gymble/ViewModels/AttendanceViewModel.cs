using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gymble.Models;
using Gymble.Services;
using System.Collections.ObjectModel;

namespace Gymble.ViewModels
{
    public partial class AttendanceViewModel : ObservableObject
    {
        private readonly IMemberService _memberService;
        private readonly IMembershipService _membershipService;
        private readonly IAttendanceService _attendanceService;

        public string PageTitle { get; set; } = "출석 관리";
        public ObservableCollection<AttendanceViewItem> Attendances { get; } = new();
        public ObservableCollection<Member> MemberSearchResults { get; } = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CheckInCommand))]
        private Member? selectedMember;

        [ObservableProperty]
        private string searchInput = string.Empty;

        [ObservableProperty]
        private DateTime selectedDate = DateTime.Today;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CheckInCommand))]
        [NotifyCanExecuteChangedFor(nameof(SearchMemberCommand))]
        [NotifyCanExecuteChangedFor(nameof(LoadAttendancesCommand))]
        private bool isBusy;

        [ObservableProperty]
        private string? errorMessage;

        [ObservableProperty]
        private string? resultMessage;

        [ObservableProperty]
        private string usableMembershipSummary = "회원을 선택해 주세요.";

        public IAsyncRelayCommand SearchMemberCommand { get; }
        public IAsyncRelayCommand CheckInCommand { get; }
        public IAsyncRelayCommand LoadAttendancesCommand { get; }
        public IRelayCommand PreviousDateCommand { get; }
        public IRelayCommand NextDateCommand { get; }
        public IRelayCommand TodayCommand { get; }

        public AttendanceViewModel(
            IMemberService memberService,
            IMembershipService membershipService,
            IAttendanceService attendanceService)
        {
            _memberService = memberService ?? throw new ArgumentNullException(nameof(memberService));
            _membershipService = membershipService ?? throw new ArgumentNullException(nameof(membershipService));
            _attendanceService = attendanceService ?? throw new ArgumentNullException(nameof(attendanceService));

            SearchMemberCommand = new AsyncRelayCommand(SearchMembersAsync, CanRunCommand);
            CheckInCommand = new AsyncRelayCommand(CheckInAsync, CanCheckIn);
            LoadAttendancesCommand = new AsyncRelayCommand(LoadAttendancesAsync, CanRunCommand);
            PreviousDateCommand = new RelayCommand(() => ChangeDate(-1));
            NextDateCommand = new RelayCommand(() => ChangeDate(1));
            TodayCommand = new RelayCommand(() => SetDate(DateTime.Today));

            _ = LoadAttendancesAsync();
        }

        partial void OnSelectedMemberChanged(Member? value)
        {
            _ = LoadSelectedMemberMembershipSummaryAsync(value?.Id);
        }

        partial void OnSelectedDateChanged(DateTime value)
        {
            _ = LoadAttendancesAsync();
        }

        private async Task SearchMembersAsync()
        {
            IsBusy = true;
            ErrorMessage = null;
            ResultMessage = null;

            try
            {
                var result = await _memberService.SearchAsync(new MemberSearch
                {
                    NameOrPhone = string.IsNullOrWhiteSpace(SearchInput) ? null : SearchInput.Trim(),
                    Page = 1,
                    PageSize = 50,
                    SortBy = "id",
                    Desc = false
                });

                MemberSearchResults.Clear();
                foreach (var member in result.Rows)
                    MemberSearchResults.Add(member);

                SelectedMember = MemberSearchResults.FirstOrDefault();
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

        private async Task CheckInAsync()
        {
            if (SelectedMember == null)
                return;

            IsBusy = true;
            ErrorMessage = null;
            ResultMessage = null;

            try
            {
                var result = await _attendanceService.CheckInAsync(SelectedMember.Id, DateTime.Now);

                if (result.Success)
                {
                    ResultMessage = CreateResultMessage(result);
                    await LoadAttendancesAsync();
                    await LoadSelectedMemberMembershipSummaryAsync(SelectedMember.Id);
                }
                else
                {
                    ErrorMessage = result.Message;
                }
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

        private async Task LoadAttendancesAsync()
        {
            IsBusy = true;
            ErrorMessage = null;

            try
            {
                var attendances = await _attendanceService.GetByDateAsync(SelectedDate);

                Attendances.Clear();
                foreach (var attendance in attendances)
                    Attendances.Add(attendance);
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

        private async Task LoadSelectedMemberMembershipSummaryAsync(int? memberId)
        {
            if (!memberId.HasValue || memberId.Value <= 0)
            {
                UsableMembershipSummary = "회원을 선택해 주세요.";
                return;
            }

            try
            {
                var memberships = await _membershipService.GetByMemberIdAsync(memberId.Value);

                if (SelectedMember?.Id != memberId.Value)
                    return;

                var gymMembership = memberships
                    .Where(x => x.Category == ProductCategory.Gym)
                    .Where(x => x.Status is MembershipStatus.Active or MembershipStatus.Pending)
                    .OrderBy(x => x.Status == MembershipStatus.Active ? 0 : 1)
                    .ThenBy(x => x.EndDate ?? DateTime.MaxValue)
                    .ThenBy(x => x.PurchasedAt)
                    .FirstOrDefault();

                UsableMembershipSummary = gymMembership == null
                    ? "사용 가능한 헬스 이용권이 없습니다."
                    : $"{gymMembership.ProductName} / {gymMembership.StatusText} / {gymMembership.PeriodText} / {gymMembership.UsageText}";
            }
            catch (Exception ex)
            {
                if (SelectedMember?.Id == memberId.Value)
                    UsableMembershipSummary = ex.Message;
            }
        }

        private void ChangeDate(int days)
        {
            SelectedDate = SelectedDate.AddDays(days);
        }

        private void SetDate(DateTime date)
        {
            SelectedDate = date.Date;
        }

        private bool CanRunCommand()
        {
            return !IsBusy;
        }

        private bool CanCheckIn()
        {
            return !IsBusy && SelectedMember != null;
        }

        private static string CreateResultMessage(CheckInResult result)
        {
            var usageText = result.MembershipResults.Count == 0
                ? ""
                : " " + string.Join(" ", result.MembershipResults.Select(x => x.Message));

            return $"{result.MemberName} 회원 출석이 완료되었습니다.{usageText}";
        }
    }
}
