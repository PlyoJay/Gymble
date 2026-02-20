using CommunityToolkit.Mvvm.ComponentModel;
using Gymble.Controls;
using Gymble.Models;
using Gymble.Services;
using System.Windows;
using System.Windows.Input;

namespace Gymble.ViewModels.Popup
{
    public partial class EditMemberViewModel : ObservableObject
    {
        [ObservableProperty]
        private Member targetMember;

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private string selectedGender;

        [ObservableProperty]
        private string phoneFirst;

        [ObservableProperty]
        private string phoneMiddle;

        [ObservableProperty]
        private string phoneLast;

        [ObservableProperty]
        private string selectedMemberState;

        [ObservableProperty]
        private string memo;

        [ObservableProperty]
        private bool isBusy;

        public List<string> MemberStateList { get; set; } = new List<string>();

        public ICommand? CloseCommand { get; }
        public ICommand? EditCommand { get; }

        private const string StateNormal = "정상";
        private const string StateDormant = "휴면";
        private const string StateSuspended = "정지";
        private const string StateWithdrawn = "탈퇴";

        #region Fields

        private readonly IMemberService _memberService;

        #endregion

        #region Events

        public event Action<bool>? RequestClose;

        #endregion

        public EditMemberViewModel(IMemberService memberService)
        {
            _memberService = memberService;

            InitStateComboBox();

            CloseCommand = new RelayCommand(w => Close(false));
            EditCommand = new RelayCommand(async _ => await EditMemberAsync(), _ => CanUpdate() && !IsBusy);
        }

        public void Initialize(Member member)
        {
            TargetMember = new Member
            {
                Id = member.Id,
                Name = member.Name,
                Gender = member.Gender,
                PhoneNumber = member.PhoneNumber,
                BirthDate = member.BirthDate,
                RegisterDate = member.RegisterDate,
                Memo = member.Memo
            };

            MemberInit();
        }

        private void MemberInit()
        {
            Name = TargetMember.Name;
            SelectedGender = TargetMember.Gender;

            PhoneFirst = TargetMember.PhoneNumber!.Substring(0, 3);
            PhoneMiddle = TargetMember.PhoneNumber!.Substring(3, 4);
            PhoneLast = TargetMember.PhoneNumber!.Substring(7, 4);

            SelectedMemberState = Utils.Utils.ConvertMeberStateToKor(TargetMember.Status);

            Memo = TargetMember.Memo;
        }

        private void InitStateComboBox()
        {
            MemberStateList.Clear();

            MemberStateList.Add(StateNormal);
            MemberStateList.Add(StateDormant);
            MemberStateList.Add(StateSuspended);
            MemberStateList.Add(StateWithdrawn);
        }

        private async Task EditMemberAsync()
        {
            if (!CanUpdate())
            {
                MessageBox.Show("필수 입력값을 확인해주세요.");
                return;
            }

            try
            {
                IsBusy = true;

                var phone = $"{PhoneFirst}{PhoneMiddle}{PhoneLast}";

                Member updatedMember = new Member()
                {
                    Id = TargetMember.Id,
                    Name = Name,
                    Gender = SelectedGender,
                    PhoneNumber = phone,
                    BirthDate = TargetMember.BirthDate,
                    Status = Utils.Utils.ConvertKorToMeberState(SelectedMemberState),
                    RegisterDate = TargetMember.RegisterDate,
                    Memo = Memo
                };

                await _memberService.UpdateAsync(updatedMember);
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

        private bool CanUpdate()
        {
            return !string.IsNullOrWhiteSpace(Name)
                && !string.IsNullOrWhiteSpace(SelectedGender)
                && !string.IsNullOrWhiteSpace(PhoneFirst)
                && !string.IsNullOrWhiteSpace(PhoneMiddle)
                && !string.IsNullOrWhiteSpace(PhoneLast)
                && !string.IsNullOrWhiteSpace(SelectedMemberState);
        }
    }
}
