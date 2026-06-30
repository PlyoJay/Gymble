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

            cmd.CommandText = SqlProductComponentQuery.CREATE_PRODUCT_COMPONENT_TABLE;
            cmd.ExecuteNonQuery();

            EnsureProductSchema(conn);

            cmd.CommandText = SqlCodeSequenceQuery.CREATE_CODE_SEQUENCE_TABLE;
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
            EnsurePurchaseSchema(conn);
            conn.Execute(SqlPurchaseQuery.CREATE_PURCHASE_MEMBER_ID_INDEX);
            conn.Execute(SqlPurchaseQuery.CREATE_PURCHASE_ITEM_PURCHASE_ID_INDEX);
        }

        private void EnsurePurchaseSchema(SQLiteConnection conn)
        {
            EnsureColumn(conn, "tb_purchase_item", "fixed_start_date", "TEXT");
        }

        private void EnsureProductSchema(SQLiteConnection conn)
        {
            EnsureColumn(conn, "tb_product", "code", "TEXT NOT NULL DEFAULT ''");
            EnsureColumn(conn, "tb_product", "name", "TEXT NOT NULL DEFAULT ''");
            EnsureColumn(conn, "tb_product", "sale_type", "INTEGER NOT NULL DEFAULT 0");
            EnsureColumn(conn, "tb_product", "price", "INTEGER NOT NULL DEFAULT 0");
            EnsureColumn(conn, "tb_product", "status", "INTEGER NOT NULL DEFAULT 0");
            EnsureColumn(conn, "tb_product", "is_favorite", "INTEGER NOT NULL DEFAULT 0");
            EnsureColumn(conn, "tb_product", "note", "TEXT");
            EnsureColumn(conn, "tb_product", "created_at", "TEXT NOT NULL DEFAULT ''");
            EnsureColumn(conn, "tb_product", "updated_at", "TEXT NOT NULL DEFAULT ''");

            EnsureColumn(conn, "tb_product_component", "product_id", "INTEGER NOT NULL DEFAULT 0");
            EnsureColumn(conn, "tb_product_component", "name", "TEXT NOT NULL DEFAULT ''");
            EnsureColumn(conn, "tb_product_component", "category", "INTEGER NOT NULL DEFAULT 0");
            EnsureColumn(conn, "tb_product_component", "usage_type", "INTEGER NOT NULL DEFAULT 0");
            EnsureColumn(conn, "tb_product_component", "usage_value", "INTEGER NOT NULL DEFAULT 0");
            EnsureColumn(conn, "tb_product_component", "start_type", "INTEGER NOT NULL DEFAULT 0");
            EnsureColumn(conn, "tb_product_component", "fixed_start_date", "TEXT");
            EnsureColumn(conn, "tb_product_component", "note", "TEXT");
        }

        private static void EnsureColumn(SQLiteConnection conn, string tableName, string columnName, string columnDefinition)
        {
            if (ColumnExists(conn, tableName, columnName))
                return;

            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnDefinition};";
            cmd.ExecuteNonQuery();
        }

        private static bool ColumnExists(SQLiteConnection conn, string tableName, string columnName)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"PRAGMA table_info({tableName});";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (string.Equals(reader["name"]?.ToString(), columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
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
