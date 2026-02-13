using Gymble.ViewModels.Popup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Gymble.Views.Popup
{
    /// <summary>
    /// EditMemberWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class EditMemberWindow : Window
    {
        public EditMemberWindow()
        {
            InitializeComponent();

            Loaded += EditMemberWindow_Loaded;
        }

        private void EditMemberWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is EditMemberViewModel vm)
            {
                vm.RequestClose += Vm_RequestClose;
            }
        }

        private void Vm_RequestClose(bool result)
        {
            DialogResult = result;
            Close();
        }

        private void NumberOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb.Text.Length == tb.MaxLength)
            {
                tb.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }
    }
}
