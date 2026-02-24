using Gymble.ViewModels.Popup;
using System.Windows;

namespace Gymble.Views.Popup
{
    /// <summary>
    /// AddProductWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AddProductWindow : Window
    {
        public AddProductWindow()
        {
            InitializeComponent();

            Loaded += AddProductWindow_Loaded;
        }

        private void AddProductWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is AddProductViewModel vm)
            {
                vm.RequestClose += Vm_RequestClose;
            }
        }

        private void Vm_RequestClose(bool result)
        {
            DialogResult = result;
            Close();
        }
    }
}
