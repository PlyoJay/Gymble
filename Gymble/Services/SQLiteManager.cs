using Dapper;
using Gymble.Models;
using Gymble.Repositories;
using Gymble.Utils;
using System.Data.SQLite;
using System.IO;

namespace Gymble.Services
{
    public class SQLiteManager
    {
        private static SQLiteManager? _instance;
        public static SQLiteManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SQLiteManager();

                return _instance;
            }
        }

        public string DatabaseName = Constants.Database.FileName;
        public string FolderName = Constants.Database.FolderName;

        private static readonly object _lock = new object();

        public SQLiteManager()
        {
            EnsureCreated();
        }

        private string GetFolderPath()
            => Path.Combine(Utils.Utils.CurrentDirectory, FolderName);

        private string GetFilePath()
            => Path.Combine(GetFolderPath(), DatabaseName);

        private void EnsureDbFile()
        {
            Directory.CreateDirectory(GetFolderPath());

            string filePath = GetFilePath();
            if (!File.Exists(filePath))
                SQLiteConnection.CreateFile(filePath);
        }

        public SQLiteConnection OpenConnection()
        {
            var conn = new SQLiteConnection($"Data Source={GetFilePath()}");
            conn.Open();
            return conn;
        }

        public void EnsureCreated()
        {
            EnsureDbFile();

            using var conn = OpenConnection();
            using var cmd = new SQLiteCommand(conn);

            // ✅ 여기서 "모든 테이블/인덱스"를 책임지고 만든다
            cmd.CommandText = SqlMemberQuery.CREATE_MEMBER_TABLE;
            cmd.ExecuteNonQuery();

            cmd.CommandText = SqlProductQuery.CREATE_PRODUCT_TABLE;
            cmd.ExecuteNonQuery();

            cmd.CommandText = SqlProductQuery.CREATE_CODE_SEQUENCE_TABLE;
            cmd.ExecuteNonQuery();

            cmd.CommandText = SqlMemberMembershipQuery.CREATE_MEMBER_MEMBERSHIP_TABLE;
            cmd.ExecuteNonQuery();

            cmd.CommandText = SqlAttendanceQuery.CREATE_ATTENDANCE_TABLE;
            cmd.ExecuteNonQuery();

            EnsurePurchaseTables(conn);

            // ✅ (추천) 하루 1회 체크인 강제 인덱스도 여기서
            // cmd.CommandText = SqlAttendanceQuery.CREATE_ATTENDANCE_UNIQUE_INDEX;
            // cmd.ExecuteNonQuery();
        }

        private void EnsurePurchaseTables(SQLiteConnection conn)
        {
            conn.Execute(SqlPurchaseQuery.CREATE_PURCHASE_TABLE);
            conn.Execute(SqlPurchaseQuery.CREATE_PURCHASE_ITEM_TABLE);
            conn.Execute(SqlPurchaseQuery.CREATE_PURCHASE_MEMBER_ID_INDEX);
            conn.Execute(SqlPurchaseQuery.CREATE_PURCHASE_ITEM_PURCHASE_ID_INDEX);
        }

        public Func<SQLiteConnection> ConnectionFactory()
        {
            return () =>
            {
                var conn = new SQLiteConnection($"Data Source={GetFilePath()}");
                conn.Open();
                return conn;
            };
        }
    }
}
