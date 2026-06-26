using Gymble.Models;
using Gymble.Repositories;
using Gymble.Services;
using Gymble.ViewModels;
using Gymble.ViewModels.Popup;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Windows;

namespace Gymble
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();

            var sqliteManager = SQLiteManager.Instance;
            sqliteManager.EnsureCreated();

            services.AddSingleton(sqliteManager);

            services.AddTransient<Func<SQLiteConnection>>(_ =>
            {
                return sqliteManager.ConnectionFactory();
            });

            // Repository
            services.AddTransient<IMemberRepository, MemberRepository>();
            services.AddTransient<IAttendanceRepository, AttendanceRepository>();
            services.AddTransient<IProductRepository, ProductRepository>();
            services.AddTransient<IPurchaseRepository, PurchaseRepository>();

            // Service
            services.AddTransient<IMemberService, MemberService>();
            services.AddTransient<IProductService, ProductService>();
            services.AddTransient<IProductCodeGenerator, ProductCodeGenerator>();
            services.AddTransient<IPurchaseService, PurchaseService>();

            // ViewModel
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<MemberListViewModel>();
            services.AddTransient<AttendanceViewModel>();
            services.AddTransient<ProductViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<MainWindowViewModel>();

            services.AddTransient<AddMemberViewModel>();
            services.AddTransient<EditMemberViewModel>();
            services.AddTransient<ProductEditorViewModel>();
            services.AddTransient<PurchaseProductViewModel>();

            Services = services.BuildServiceProvider();
        }
    }
}
