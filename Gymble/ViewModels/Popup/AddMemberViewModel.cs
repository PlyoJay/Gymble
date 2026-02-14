using CommunityToolkit.Mvvm.ComponentModel;
using Gymble.Controls;
using Gymble.Models;
using Gymble.Services;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Gymble.ViewModels.Popup
{
    public partial class AddMemberViewModel : ObservableObject
    {
        public ObservableCollection<int>? Years { get; }
        public ObservableCollection<int>? Months { get; set; }

        [ObservableProperty]
        private string? memberName;

        [ObservableProperty]
        private string? selectedGender;

        [ObservableProperty]
        private string? phoneFirst;

        [ObservableProperty]
        private string? phoneMiddle;

        [ObservableProperty]
        private string? phoneLast; 

        [ObservableProperty]
        private ObservableCollection<int> days;

        [ObservableProperty]
        private int? selectedYear;

        [ObservableProperty]
        private int? selectedMonth;

        [ObservableProperty]
        private int? selectedDay;

        [ObservableProperty]
        private DateTime registerDate;

        [ObservableProperty]
        private string memo;

        [ObservableProperty]
        private bool isBusy;

        public ICommand? CloseCommand { get; }
        public ICommand? AddCommand { get; }

        #region Fields

        private readonly IMemberService _memberService;

        #endregion

        #region Events

        public event Action<bool>? RequestClose;

        #endregion

        public AddMemberViewModel(IMemberService memberService)
        {        
            _memberService = memberService;

            Years = new ObservableCollection<int>();
            Months = new ObservableCollection<int>();
            Days = new ObservableCollection<int>();

            for (int i = 0; i < 100; i++)
                Years.Add(new DateTime(DateTime.Now.Year - i, 1, 1).Year);

            for (int i = 1; i <= 12; i++)
                Months.Add(i);

            SelectedGender = "Male";

            SelectedYear = Years.FirstOrDefault(y => y.Equals(2000));
            SelectedMonth = 1;
            SelectedDay = 1;

            RegisterDate = DateTime.Today;

            CloseCommand = new RelayCommand(_ => Close(false));
            AddCommand = new RelayCommand(async _ => await AddMemberAsync(), _ => CanAdd() && !IsBusy);
        }

        private bool CanAdd()
        {
            // 전화번호 3칸도 최소한 체크하는 걸 추천
            return !string.IsNullOrWhiteSpace(MemberName)
                && !string.IsNullOrWhiteSpace(SelectedGender)
                && SelectedYear.HasValue && SelectedMonth.HasValue && SelectedDay.HasValue
                && !string.IsNullOrWhiteSpace(PhoneFirst)
                && !string.IsNullOrWhiteSpace(PhoneMiddle)
                && !string.IsNullOrWhiteSpace(PhoneLast);
        }

        private void Close(bool result)
        {
            if (IsBusy) return;
            RequestClose?.Invoke(result);
        }

        private async Task AddMemberAsync()
        {
            if (!CanAdd())
            {
                MessageBox.Show("필수 입력값을 확인해주세요.");
                return;
            }

            try
            {
                IsBusy = true;

                var phone = $"{PhoneFirst}{PhoneMiddle}{PhoneLast}";
                var birthDate = new DateTime(SelectedYear!.Value, SelectedMonth!.Value, SelectedDay!.Value);

                var member = new Member
                {
                    Name = MemberName,
                    Gender = SelectedGender,
                    PhoneNumber = phone,
                    BirthDate = birthDate,
                    RegisterDate = RegisterDate,
                    State = MemberState.Normal,
                    Memo = Memo
                };

                await _memberService.AddAsync(member);

                // 저장 성공 후 닫기
                DialogHost.Close("MainDialog", "Ok");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }            
        }

        private void UpdateDays()
        {
            Days.Clear();

            if (!SelectedYear.HasValue || !SelectedMonth.HasValue)
                return;

            int y = SelectedYear.Value;
            int m = SelectedMonth.Value;

            int daysInMonth = DateTime.DaysInMonth(y, m);
            for (int d = 1; d <= daysInMonth; d++)
                Days.Add(d);

            if (SelectedDay.HasValue && SelectedDay.Value > daysInMonth)
                SelectedDay = daysInMonth;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
