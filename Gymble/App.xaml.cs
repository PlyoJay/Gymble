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

            // DB 연결 (예시) - 네 SQLiteManager가 있으면 거기서 연결 가져오는 식으로 바꿔도 됨
            services.AddSingleton(_ =>
            {
                var conn = new SQLiteConnection("Data Source=gymble.db;Version=3;");
                conn.Open();
                return conn;
            });

            services.AddSingleton(SQLiteManager.Instance);

            services.AddTransient<Func<SQLiteConnection>>(sp =>
            {
                var mgr = SQLiteManager.Instance;
                return mgr.ConnectionFactory();
            });

            // Repository
            services.AddSingleton<IMemberRepository, MemberRepository>();
            services.AddSingleton<IAttendanceRepository, AttendanceRepository>();

            // Service
            services.AddSingleton<IMemberService, MemberService>();
            //services.AddSingleton<IAttendanceService, AttendanceService>(); // 나중에 추가

            // ViewModel
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<MemberListViewModel>();
            services.AddTransient<AttendanceViewModel>();
            services.AddTransient<ProductViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<MainWindowViewModel>();

            services.AddTransient<AddMemberViewModel>();
            services.AddTransient<EditMemberViewModel>();

            Services = services.BuildServiceProvider();
        }
    }
}
