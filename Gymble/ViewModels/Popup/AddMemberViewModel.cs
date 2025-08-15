using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Gymble.ViewModels.Popup
{
    public class AddMemberViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<int>? Years { get; }
        public ObservableCollection<int>? Months { get; }
        public ObservableCollection<int>? Days { get; set; }

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

        private int? _phoneFirst;
        public int? PhoneFirst
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

        private int? _phoneMIddle;
        public int? PhoneMiddle
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

        private int? _phoneLast;
        public int? PhoneLast
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

                    Days = GenerateDays(SelectedYear, SelectedMonth);
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

        private DateTime? _registerDate;
        public DateTime? RegisterDate
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
                if (string.IsNullOrEmpty(value))
                {
                    _memo = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand? CloseCommand { get; }
        public ICommand? AddCommand { get; }

        public AddMemberViewModel()
        {
            int currentYear = DateTime.Now.Year;
            
            
            Years = new ObservableCollection<int>();
            Months = new ObservableCollection<int>();

            for (int i = 0; i < 100; i++)
                Years.Add(new DateTime(currentYear - i, 1, 1).Year);

            for (int i = 1; i <= 12; i++)
                Months.Add(i);

            RegisterDate = DateTime.Today;
        }

        private ObservableCollection<int> GenerateDays(int? year, int? month)
        {
            var days = new ObservableCollection<int>();

            // 해당 연·월의 마지막 날 구하기
            int daysInMonth = DateTime.DaysInMonth((int)year, (int)month);

            for (int day = 1; day <= daysInMonth; day++)
            {
                days.Add(day);
            }

            return days;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
