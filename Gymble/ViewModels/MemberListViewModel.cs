using Gymble.Controls;
using Gymble.Models;
using Gymble.Services;
using Gymble.ViewModels.Popup;
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

        private Member _selectedMember;
        public Member SelectedMember
        {
            get => _selectedMember;
            set
            {
                if (_selectedMember != value)
                {
                    _selectedMember = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand? SearchCommand { get; }
        public ICommand? AddCommand { get; }
        public ICommand? EditCommand { get; }
        public ICommand? DeleteCommand { get; }

        public MemberListViewModel()
        {
            MemberList = new ObservableCollection<Member>(Datas.GetMemberList());

            AddCommand = new RelayCommand(AddMember);
            DeleteCommand = new RelayCommand(DeleteMember);
            EditCommand = new RelayCommand(EditMember);
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

        private void DeleteMember(object obj)
        {
            var msgResult = MessageBox.Show("정말로 삭제하겠습니까?", "경고", MessageBoxButton.OKCancel);

            if (msgResult == MessageBoxResult.No) return;
            
            SQLiteManager.Instance.DeleteMember(SelectedMember);
            SQLiteManager.Instance.UseMemberRepository();
            UpdateMemberList();            
        }

        private void EditMember(object obj)
        {
            EditMemberViewModel editMemberViewModel = new EditMemberViewModel(SelectedMember);
            EditMemberWindow editMemberWindow = new EditMemberWindow()
            {
                DataContext = editMemberViewModel,
            };
            editMemberWindow.ShowDialog();
        }

        private void UpdateMemberList()
        {
            MemberList!.Clear();
            foreach (var m in Datas.GetMemberList()) MemberList.Add(m);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
