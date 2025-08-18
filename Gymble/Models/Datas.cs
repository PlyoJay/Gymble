using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gymble.Models
{
    public static class Datas
    {
        public static ObservableCollection<Member> MemberList { get; set; }

        static Datas()
        {
            MemberList = new ObservableCollection<Member>();
        }

        public static void SetMemberList(List<Member> memberList)
        {
            MemberList.Clear();
            foreach (Member m in memberList)            
                MemberList.Add(m);         
        }

        public static ObservableCollection<Member> GetMemberList()
        {
            return MemberList;
        }
    }
}
