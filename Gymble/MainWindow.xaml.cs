using Gymble.Services;
using Gymble.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Gymble
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SQLiteManager sqliteManager = null;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = App.Services.GetRequiredService<MainWindowViewModel>();

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CreateDataFolder();

            //sqliteManager = new SQLiteManager();
            //sqliteManager.GetAllRepositories();
        }

        private void CreateDataFolder()
        {
            string dataFolderPath = System.IO.Path.Combine(Utils.Utils.CurrentDirectory, "data_folder");
            if (Directory.Exists(dataFolderPath))
                return;

            Directory.CreateDirectory(dataFolderPath);
        }
    }
}