using Gymble.Controls;
using Gymble.Models;
using Gymble.Services;
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
using System.Windows.Media;
using System.Xml.Linq;

namespace Gymble.ViewModels.Popup
{
    public class EditMemberViewModel : INotifyPropertyChanged
    {
        public Member TargetMember { get; set; }

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

        public ObservableCollection<int>? Years { get; }
        public ObservableCollection<int>? Months { get; set; }

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

        public ICommand? CloseCommand { get; }
        public ICommand? EditCommand { get; }

        public EditMemberViewModel(Member member)
        {
            this.TargetMember = member;

            Years = new ObservableCollection<int>();
            Months = new ObservableCollection<int>();
            Days = new ObservableCollection<int>();

            for (int i = 0; i < 100; i++)
                Years.Add(new DateTime(DateTime.Now.Year - i, 1, 1).Year);

            for (int i = 1; i <= 12; i++)
                Months.Add(i);

            MemberInit();

            CloseCommand = new RelayCommand<Window>(w => Close(w, true), w => w != null);
            EditCommand = new RelayCommand<Window>(w => EditMember(w), w => CanUpdate());
        }

        private void MemberInit()
        {
            Name = TargetMember.Name;
            SelectedGender = TargetMember.Gender;

            PhoneFirst = TargetMember.PhoneNumber!.Split('-')[0];
            PhoneMiddle = TargetMember.PhoneNumber!.Split('-')[1];
            PhoneLast = TargetMember.PhoneNumber!.Split('-')[2];

            SelectedYear = TargetMember.BirthDate.Year;
            SelectedMonth = TargetMember.BirthDate.Month;
            SelectedDay = TargetMember.BirthDate.Day;

            Memo = TargetMember.Memo;
        }

        private void EditMember(Window w)
        {
            var phone = $"{PhoneFirst}-{PhoneMiddle}-{PhoneLast}";

            var birthDate = new DateTime((int)SelectedYear!, (int)SelectedMonth!, (int)SelectedDay!);

            Member UpdatedMember = new Member()
            {
                Id = TargetMember.Id,
                Name = Name,
                Gender = SelectedGender,
                PhoneNumber = phone,
                BirthDate = birthDate,
                RegisterDate = TargetMember.RegisterDate,
                Memo = Memo
            };

            SQLiteManager.Instance.UpdateMember(UpdatedMember);

            if (w != null)
            {
                w.DialogResult = true;
                w.Close();
            }
        }

        private void Close(Window w, bool result)
        {
            w.DialogResult = result;
            w.Close();
        }

        private bool CanUpdate()
        {
            return !string.IsNullOrWhiteSpace(TargetMember.Name)
                && !string.IsNullOrWhiteSpace(SelectedGender)
                && SelectedYear.HasValue && SelectedMonth.HasValue && SelectedDay.HasValue;
        }

        private void UpdateDays()
        {
            Days.Clear();
            int daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, (int)SelectedMonth! == 0 ? 1 : (int)SelectedMonth);
            for (int i = 1; i <= daysInMonth; i++)
                Days.Add(i);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
