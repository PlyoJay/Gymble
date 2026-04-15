using CommunityToolkit.Mvvm.ComponentModel;
using Gymble.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Gymble.ViewModels.Popup
{
    public partial class PurchaseProductViewModel : ObservableObject
    {
        [ObservableProperty]
        private ProductCategory selectedCategory = ProductCategory.Gym;

        [ObservableProperty]
        private Member targetMember;

        [ObservableProperty]
        private DateTime selectedDate = DateTime.Today;

        public PurchaseProductViewModel()
        {

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
                Status = member.Status,
                Memo = member.Memo
            };
        }
    }
}
