using Gymble.ViewModels.Popup;
using System.Windows;

namespace Gymble.Views.Popup
{
    public partial class PurchaseProductWindow : Window
    {
        private PurchaseProductViewModel? _viewModel;

        public PurchaseProductWindow()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;
            Closed += OnClosed;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Unsubscribe();

            _viewModel = e.NewValue as PurchaseProductViewModel;
            if (_viewModel != null)
                _viewModel.CloseRequested += OnCloseRequested;
        }

        private void OnCloseRequested(bool? dialogResult)
        {
            DialogResult = dialogResult;
            Close();
        }

        private void OnClosed(object? sender, EventArgs e)
        {
            Unsubscribe();
            DataContextChanged -= OnDataContextChanged;
            Closed -= OnClosed;
        }

        private void Unsubscribe()
        {
            if (_viewModel != null)
                _viewModel.CloseRequested -= OnCloseRequested;

            _viewModel = null;
        }
    }
}
