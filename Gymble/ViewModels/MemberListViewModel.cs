using Gymble.Controls;
using Gymble.Models;
using Gymble.Services;
using Gymble.ViewModels.Popup;
using Gymble.Views;
using Gymble.Views.Popup;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
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
        public string PageTitle { get; set; } = "회원 관리";

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

        #region Fields

        private readonly IMemberService _memberService;

        #endregion

        public MemberListViewModel(IMemberService memberService)
        {
            _memberService = memberService;

            MemberList = new ObservableCollection<Member>(Datas.GetMemberList());

            AddCommand = new RelayCommand(AddMember);
            DeleteCommand = new RelayCommand(DeleteMember);
            EditCommand = new RelayCommand(EditMember);
        }

        private void AddMember(object obj)
        {
            var vm = App.Services.GetRequiredService<AddMemberViewModel>();

            var view = new AddMemberView { DataContext = vm };

            DialogHost.Show(view, "MainDialog");
        }

        private void DeleteMember(object obj)
        {
            var msgResult = MessageBox.Show("정말로 삭제하겠습니까?", "경고", MessageBoxButton.OKCancel);

            if (msgResult == MessageBoxResult.No) return;
            
            //SQLiteManager.Instance.DeleteMember(SelectedMember);
            UpdateMemberList();            
        }

        private void EditMember(object obj)
        {
            if (SelectedMember == null) return;

            EditMemberViewModel editMemberViewModel = new EditMemberViewModel(SelectedMember);
            EditMemberWindow editMemberWindow = new EditMemberWindow()
            {
                DataContext = editMemberViewModel,
            };
            editMemberWindow.ShowDialog();

            UpdateMemberList();
        }

        private void UpdateMemberList()
        {
            //SQLiteManager.Instance.UseMemberRepository();
            MemberList!.Clear();
            foreach (var m in Datas.GetMemberList()) MemberList.Add(m);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
