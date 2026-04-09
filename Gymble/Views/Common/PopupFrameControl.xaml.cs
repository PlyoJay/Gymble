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

namespace Gymble.Views.Common
{
    /// <summary>
    /// PopupFrameControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PopupFrameControl : UserControl
    {
        public PopupFrameControl()
        {
            InitializeComponent();
        }

        public string TitleText
        {
            get => (string)GetValue(TitleTextProperty);
            set => SetValue(TitleTextProperty, value);
        }

        public static readonly DependencyProperty TitleTextProperty =
            DependencyProperty.Register(nameof(TitleText), typeof(string), typeof(PopupFrameControl), new PropertyMetadata(string.Empty));

        public object BodyContent
        {
            get => GetValue(BodyContentProperty);
            set => SetValue(BodyContentProperty, value);
        }

        public static readonly DependencyProperty BodyContentProperty =
            DependencyProperty.Register(nameof(BodyContent), typeof(object), typeof(PopupFrameControl), new PropertyMetadata(null));

        public object FooterContent
        {
            get => GetValue(FooterContentProperty);
            set => SetValue(FooterContentProperty, value);
        }

        public static readonly DependencyProperty FooterContentProperty =
            DependencyProperty.Register(nameof(FooterContent), typeof(object), typeof(PopupFrameControl), new PropertyMetadata(null));

        public Thickness BodyMargin
        {
            get => (Thickness)GetValue(BodyMarginProperty);
            set => SetValue(BodyMarginProperty, value);
        }

        public static readonly DependencyProperty BodyMarginProperty =
            DependencyProperty.Register(nameof(BodyMargin), typeof(Thickness), typeof(PopupFrameControl), new PropertyMetadata(new Thickness(20, 15, 20, 0)));

        public Thickness FooterMargin
        {
            get => (Thickness)GetValue(FooterMarginProperty);
            set => SetValue(FooterMarginProperty, value);
        }

        public static readonly DependencyProperty FooterMarginProperty =
            DependencyProperty.Register(nameof(FooterMargin), typeof(Thickness), typeof(PopupFrameControl), new PropertyMetadata(new Thickness(20, 0, 20, 0)));

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }
    }
}
