using Gymble.Controls;
using Gymble.Models;
using Gymble.Services;
using Gymble.Views.Popup;
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
    public class MemberListViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Member>? MemberList { get; }

        public ICommand? SearchCommand { get; }
        public ICommand? AddCommand { get; }
        public ICommand? EditCommand { get; }
        public ICommand? DeleteCommand { get; }

        public MemberListViewModel()
        {
            MemberList = new ObservableCollection<Member>(Datas.GetMemberList());

            AddCommand = new RelayCommand(AddMember);
        }

        private void AddMember(object obj)
        {
            AddMemberWIndow addMemberWIndow = new AddMemberWIndow();
            var isAddMemberSuccess = addMemberWIndow.ShowDialog();

            if (isAddMemberSuccess == false)
            {
                MessageBox.Show("회원 추가 실패");
                return;
            }

            SQLiteManager.Instance.UseMemberRepository();
            UpdateMemberList();
        }

        private void UpdateMemberList()
        {
            MemberList.Clear();
            foreach (var m in Datas.GetMemberList())
            {
                MemberList.Add(m);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
