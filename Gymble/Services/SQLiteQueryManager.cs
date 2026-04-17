using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gymble.Services
{
    public class SqlMemberQuery
    {
        public static string ID = "id";
        public static string NAME = "name";
        public static string GENDER = "gender";
        public static string PHONENUMBER = "phone_number";
        public static string BIRTHDATE = "birthdate";
        public static string REGISTER_DATE = "register_date";
        public static string MEMO = "memo";

        public static string CREATE_MEMBER_TABLE = "CREATE TABLE IF NOT EXISTS [tb_member] " +
                        "(" +
                            "[id] INTEGER PRIMARY KEY AUTOINCREMENT, " +
                            "[name] TEXT NOT NULL,  " +
                            "[gender] TEXT NOT NULL,  " +
                            "[phone_number] TEXT,  " +
                            "[birthdate] TEXT,  " +
                            "[register_date] TEXT NOT NULL,  " +
                            "[status] TEXT NOT NULL,  " +
                            "[memo] TEXT  " +
                        ")";
    }

    public class SqlProductQuery
    {
        public static string ID = "id";
        public static string NAME = "name";
        public static string TYPE = "type";
        public static string DURATION_DAYS = "duration_days";
        public static string TOTAL_COUNT = "total_count";
        public static string PRICE = "price";

        public static string CREATE_PRODUCT_TABLE = @"
            CREATE TABLE IF NOT EXISTS tb_product (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                code TEXT NOT NULL,
                name TEXT NOT NULL,
                category INTEGER NOT NULL,
                price INTEGER NOT NULL,
                usage_type INTEGER NOT NULL,
                usage_value INTEGER NOT NULL,
                start_type INTEGER NOT NULL,
                fixed_start_date TEXT,
                status INTEGER NOT NULL,
                is_favorite INTEGER NOT NULL DEFAULT 0,
                note TEXT,
                created_at TEXT NOT NULL,
                updated_at TEXT NOT NULL
            );
            ";

        public const string INSERT_PRODUCT = @"
            INSERT INTO tb_product (code, name, category, price, usage_type, usage_value, start_type, fixed_start_date, status, is_favorite, note, created_at, updated_at)
            VALUES (@Code, @Name, @Category, @Price, @UsageType, @UsageValue, @StartType, @FixedStartDate, @Status, @IsFavorite, @Note, @CreatedAt, @UpdatedAt);
            SELECT last_insert_rowid();";

        public const string CREATE_CODE_SEQUENCE_TABLE = @"
            CREATE TABLE IF NOT EXISTS tb_code_sequence (prefix TEXT PRIMARY KEY, last_value INTEGER NOT NULL);";

        public const string UPDATE_PRODUCT = @"
            UPDATE tb_product
            SET code = @Code,
                name = @Name,
                category = @Category,
                price = @Price,
                usage_type = @UsageType,
                usage_value = @UsageValue,
                start_type = @StartType,
                fixed_start_date = @FixedStartDate,
                status = @Status,
                is_favorite = @IsFavorite,
                note = @Note,
                updated_at = @UpdatedAt
            WHERE id = @Id;";
    }

    public class SqlPurchaseQuery
    {
        public const string CREATE_PURCHASE_TABLE = "CREATE TABLE IF NOT EXISTS [tb_purchase] " +
            "(" +
            "[id] INTEGER PRIMARY KEY AUTOINCREMENT, " +
            "[member_id] INTEGER NOT NULL,  " +
            "[total_amount] INTEGER NOT NULL,  " +
            "[discount_amount] INTEGER NOT NULL,  " +
            "[final_amount] INTEGER NOT NULL,  " +
            "[payment_method] INTEGER NOT NULL,  " +
            "[status] INTEGER NOT NULL,  " +
            "[purchased_at] TEXT NOT NULL, " +
            "[memo] TEXT, " +
            "[created_at] TEXT NOT NULL,  " +
            "[updated_at] TEXT NOT NULL" +
            ")";

        public const string CREATE_PURCHASE_MEMBER_ID_INDEX = 
            "CREATE INDEX IF NOT EXISTS [idx_tb_purchase_member_id] ON [tb_purchase]([member_id]);";

        public const string CREATE_PURCHASE_ITEM_TABLE = "CREATE TABLE IF NOT EXISTS [tb_purchase_item] " +
            "(" +
            "[id] INTEGER PRIMARY KEY AUTOINCREMENT, " +
            "[purchase_id] INTEGER NOT NULL,  " +
            "[product_id] INTEGER NOT NULL,  " +
            "[product_code_snapshot] TEXT NOT NULL,  " +
            "[product_name_snapshot] TEXT NOT NULL,  " +
            "[category] INTEGER NOT NULL, " +
            "[usage_type] INTEGER, " +
            "[start_type] INTEGER, " +
            "[unit_price] INTEGER NOT NULL,  " +
            "[line_amount] INTEGER NOT NULL,  " +
            "[usage_value] INTEGER,  " +
            "[is_membership_item] INTEGER NOT NULL DEFAULT 0,  " +
            "[note] TEXT, " +
            "[created_at] TEXT NOT NULL,  " +
            "[updated_at] TEXT NOT NULL" +
            ")";

        public const string CREATE_PURCHASE_ITEM_PURCHASE_ID_INDEX = 
            "CREATE INDEX IF NOT EXISTS [idx_tb_purchase_item_purchase_id] ON [tb_purchase_item]([purchase_id]);";
    }

    public class SqlMemberMembershipQuery
    {
        public const string CREATE_MEMBER_MEMBERSHIP_TABLE = "CREATE TABLE IF NOT EXISTS [tb_member_membership] " +
            "(" +
            "[id] INTEGER PRIMARY KEY AUTOINCREMENT, " +
            "[member_id] INTEGER NOT NULL,  " +
            "[purchase_id] INTEGER NOT NULL,  " +
            "[purchase_item_id] INTEGER NOT NULL,  " +
            "[product_id] INTEGER NOT NULL,  " +
            "[product_code_snapshot] TEXT NOT NULL,  " +
            "[product_name_snapshot] TEXT NOT NULL,  " +
            "[category] INTEGER NOT NULL, " +
            "[usage_type] INTEGER NOT NULL, " +
            "[start_type] INTEGER NOT NULL, " +
            "[unit_price_snapshot] INTEGER NOT NULL,  " +
            "[usage_value] INTEGER NOT NULL,  " +
            "[duration_days] INTEGER,  " +
            "[total_count] INTEGER,  " +
            "[used_count] INTEGER,  " +
            "[remaining_count] INTEGER,  " +
            "[purchased_at] TEXT NOT NULL,  " +
            "[activated_at] TEXT,  " +
            "[start_date] TEXT,  " +
            "[end_date] TEXT,  " +
            "[status] INTEGER NOT NULL,  " +
            "[note] TEXT,  " +
            "[created_at] TEXT NOT NULL,  " +
            "[updated_at] TEXT NOT NULL" +
            ")";

        public const string CREATE_MEMBER_MEMBERSHIP_MEMBER_ID_INDEX = 
            "CREATE INDEX IF NOT EXISTS [idx_tb_member_membership_member_id] ON [tb_member_membership]([member_id]);";

        public const string CREATE_MEMBER_MEMBERSHIP_PURCHASE_ID_INDEX =
            "CREATE INDEX IF NOT EXISTS [idx_tb_member_membership_purchase_id] ON [tb_member_membership]([purchase_id]);";
    }

    public class SqlAttendanceQuery
    {
        public static string ID = "id";
        public static string MEMBER_ID = "member_id";
        public static string DATETIME = "datetime";

        public static string CREATE_ATTENDANCE_TABLE = "CREATE TABLE IF NOT EXISTS [tb_attendance] " +
                        "(" +
                            "[id] INTEGER PRIMARY KEY AUTOINCREMENT, " +
                            "[member_id] INTEGER NOT NULL,  " +
                            "[datetime] TEXT NOT NULL,  " +
                            "FOREIGN KEY (member_id) REFERENCES tb_member(id)" +
                        ")";
    }
}
