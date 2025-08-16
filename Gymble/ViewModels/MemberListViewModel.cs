using Gymble.Controls;
using Gymble.Models;
using Gymble.Views.Popup;
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
    public class MemberListViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Member>? MemberList { get; }

        public ICommand? SearchCommand { get; }
        public ICommand? AddCommand { get; }
        public ICommand? EditCommand { get; }
        public ICommand? DeleteCommand { get; }

        public MemberListViewModel()
        {
            AddCommand = new RelayCommand(AddMember);
        }

        private void AddMember(object obj)
        {
            AddMemberWIndow addMemberWIndow = new AddMemberWIndow();
            addMemberWIndow.ShowDialog();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
