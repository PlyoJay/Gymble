using Gymble.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Gymble.Views.Controls
{
    /// <summary>
    /// DetailMemberInfoView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DetailMemberInfoView : UserControl
    {
        public DetailMemberInfoView()
        {
            InitializeComponent();
        }

        public Member? Member
        {
            get => (Member?)GetValue(MemberProperty);
            set => SetValue(MemberProperty, value);
        }

        public static readonly DependencyProperty MemberProperty =
            DependencyProperty.Register(nameof(Member), typeof(Member),
                typeof(DetailMemberInfoView), new PropertyMetadata(null));
    }
}
