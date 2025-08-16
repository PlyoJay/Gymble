using Gymble.Models;
using Gymble.Repositories;
using Gymble.Utils;
using System.Data.SQLite;
using System.IO;

namespace Gymble.Services
{
    public class SQLiteManager
    {
        private static SQLiteManager _instance;
        public static SQLiteManager Instance
        {
            get
            {
                lock (_lock)
                {
                    return _instance ?? (_instance = new SQLiteManager());
                }
            }
        }

        public string DatabaseName = Constants.Database.FileName;
        public string FolderName = Constants.Database.FolderName;
        public SQLiteConnection connection = null;

        private static readonly object _lock = new object();

        public SQLiteManager()
        {
            CreateDatabaseFile();
            GetAllRepositories();
        }

        private string GetFolderPath()
        {
            return Path.Combine(Utils.Utils.CurrentDirectory, FolderName);
        }

        private string GetFilePath()
        {
            return Path.Combine(GetFolderPath(), DatabaseName);
        }

        private void CreateDatabaseFile()
        {
            DirectoryInfo di = new DirectoryInfo(GetFolderPath());
            if (!di.Exists)
            {
                di.Create();
                //Utils.log.Info("Create Database Folder.");
            }

            string filePath = GetFilePath();
            if (!File.Exists(filePath))
            {
                SQLiteConnection.CreateFile(GetFilePath());
                //Utils.log.Info("Create Database.");
                CreateTables();
            }
        }

        public void OpenConnection()
        {
            string s = GetFilePath();
            connection = new SQLiteConnection($"Data Source={GetFilePath()}");
            connection.Open();
        }

        public void CloseConnection()
        {
            connection?.Dispose();
            connection = null;
        }

        public void CreateTables()
        {
            OpenConnection();

            SQLiteCommand cmd = new SQLiteCommand(connection);
            cmd.CommandText = SqlMemberQuery.CREATE_MEMBER_TABLE;
            cmd.ExecuteNonQuery();

            cmd.CommandText = SqlProductQuery.CREATE_PRODUCT_TABLE;
            cmd.ExecuteNonQuery();

            cmd.CommandText = SqlMembershipQuery.CREATE_MEMBERSHIP_TABLE;
            cmd.ExecuteNonQuery();

            cmd.CommandText = SqlAttendanceQuery.CREATE_ATTENDANCE_TABLE;
            cmd.ExecuteNonQuery();

            CloseConnection();
        }

        public void UseMemberRepository()
        {
            OpenConnection();

            var repo = new MemberRepository(connection);
            var all = repo.GetAllMembers();

            CloseConnection();
        }

        public void InsertMember(Member member)
        {
            OpenConnection();

            var repo = new MemberRepository(connection);
            repo.InsertMember(member);

            CloseConnection();
        }

        public void UseMembershipRepository()
        {
            OpenConnection();

            var repo = new MembershipRepository(connection);
            var all = repo.GetAllMembership();

            CloseConnection();
        }

        public void UseAttendaceRepository()
        {
            OpenConnection();

            var repo = new AttendanceRepository(connection);
            var all = repo.GetAllAttendace();

            CloseConnection();
        }

        public void UseProductRepository()
        {
            OpenConnection();

            var repo = new ProductRepository(connection);
            var all = repo.GetAllProducts();

            CloseConnection();
        }

        public void GetAllRepositories()
        {
            UseMemberRepository();
            UseMembershipRepository();
            UseAttendaceRepository();
            UseProductRepository();
        }
    }
}
