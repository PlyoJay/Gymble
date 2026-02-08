using Gymble.Controls;
using Gymble.Models;
using Gymble.Services;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Gymble.ViewModels.Popup
{
    public class AddMemberViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<int>? Years { get; }
        public ObservableCollection<int>? Months { get; set; }

        private string? _name;
        public string? Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        private string? _selectedGender;
        public string? SelectedGender
        {
            get => _selectedGender;
            set
            {
                if (_selectedGender != value)
                {
                    _selectedGender = value;
                    OnPropertyChanged();
                }
            }
        }

        private string? _phoneFirst;
        public string? PhoneFirst
        {
            get => _phoneFirst;
            set
            {
                if (value != _phoneFirst)
                {
                    _phoneFirst = value;
                    OnPropertyChanged();
                }
            }
        }

        private string? _phoneMIddle;
        public string? PhoneMiddle
        {
            get => _phoneMIddle;
            set
            {
                if (value != _phoneMIddle)
                {
                    _phoneMIddle = value;
                    OnPropertyChanged();
                }
            }
        }

        private string? _phoneLast;
        public string? PhoneLast
        {
            get => _phoneLast;
            set
            {
                if (value != _phoneLast)
                {
                    _phoneLast = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<int> _days;
        public ObservableCollection<int> Days
        {
            get => _days;
            set
            {
                _days = value;
                OnPropertyChanged(nameof(Days));
            }
        }

        private int? _selectedYear;
        public int? SelectedYear
        {
            get => _selectedYear;
            set
            {
                if (value != _selectedYear)
                {
                    _selectedYear = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _selectedMonth;
        public int? SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                if (_selectedMonth != value)
                {
                    _selectedMonth = value;
                    OnPropertyChanged();
                    UpdateDays();
                }
            }
        }

        private int? _selectedDay;
        public int? SelectedDay
        {
            get => _selectedDay;
            set
            {
                if (_selectedDay != value)
                {
                    _selectedDay = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime _registerDate;
        public DateTime RegisterDate
        {
            get => _registerDate;
            set
            {
                if (_registerDate != value)
                {
                    _registerDate = value;
                    OnPropertyChanged();
                }
            }
        }

        private string? _memo;
        public string? Memo
        {
            get => _memo;
            set
            {
                if (value != _memo)
                {
                    _memo = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand? CloseCommand { get; }
        public ICommand? AddCommand { get; }

        #region Fields

        private readonly IMemberService _memberService;

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

            CloseCommand = new RelayCommand(_ => Close());
            AddCommand = new RelayCommand(async _ => await AddMemberAsync(), _ => CanAdd() && !IsBusy);
        }

        private bool CanAdd()
        {
            // 전화번호 3칸도 최소한 체크하는 걸 추천
            return !string.IsNullOrWhiteSpace(Name)
                && !string.IsNullOrWhiteSpace(SelectedGender)
                && SelectedYear.HasValue && SelectedMonth.HasValue && SelectedDay.HasValue
                && !string.IsNullOrWhiteSpace(PhoneFirst)
                && !string.IsNullOrWhiteSpace(PhoneMiddle)
                && !string.IsNullOrWhiteSpace(PhoneLast);
        }

        private void Close()
        {
            if (IsBusy) return;
            DialogHost.Close("MainDialog", "Cancel");
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

                var phone = $"{PhoneFirst}-{PhoneMiddle}-{PhoneLast}";
                var birthDate = new DateTime(SelectedYear!.Value, SelectedMonth!.Value, SelectedDay!.Value);

                var member = new Member
                {
                    Name = Name,
                    Gender = SelectedGender,
                    PhoneNumber = phone,
                    BirthDate = birthDate,
                    RegisterDate = RegisterDate,
                    State = MemberState.Using,
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
