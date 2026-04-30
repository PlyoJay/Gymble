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
        public static string CREATE_PRODUCT_TABLE = @"
            CREATE TABLE IF NOT EXISTS tb_product (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                code TEXT NOT NULL,
                name TEXT NOT NULL,
                sale_type INTEGER NOT NULL,
                price INTEGER NOT NULL,
                status INTEGER NOT NULL,
                is_favorite INTEGER NOT NULL DEFAULT 0,
                note TEXT,
                created_at TEXT NOT NULL,
                updated_at TEXT NOT NULL
            );
            ";

        public const string INSERT_PRODUCT = @"
            INSERT INTO tb_product
            (
                code,
                name,
                sale_type,
                price,
                status,
                is_favorite,
                note,
                created_at,
                updated_at
            )
            VALUES
            (
                @Code,
                @Name,
                @SaleType,
                @Price,
                @Status,
                @IsFavorite,
                @Note,
                @CreatedAt,
                @UpdatedAt
            );
            SELECT last_insert_rowid();
            ";

        public const string UPDATE_PRODUCT = @"
            UPDATE tb_product
            SET
                code = @Code,
                name = @Name,
                sale_type = @SaleType,
                price = @Price,
                status = @Status,
                is_favorite = @IsFavorite,
                note = @Note,
                updated_at = @UpdatedAt
            WHERE id = @Id;
            ";

        public const string DELETE_PRODUCT = @"
            DELETE FROM tb_product
            WHERE id = @ProductId;
            ";

        public const string GET_PRODUCT_BY_ID = @"
            SELECT
                id,
                code,
                name,
                sale_type AS SaleType,
                price,
                status,
                is_favorite AS IsFavorite,
                note,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM tb_product
            WHERE id = @ProductId;
            ";
    }

    public static class SqlProductComponentQuery
    {
        public const string CREATE_PRODUCT_COMPONENT_TABLE = @"
            CREATE TABLE IF NOT EXISTS tb_product_component (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                product_id INTEGER NOT NULL,
                name TEXT NOT NULL,
                category INTEGER NOT NULL,
                usage_type INTEGER NOT NULL,
                usage_value INTEGER NOT NULL,
                start_type INTEGER NOT NULL,
                fixed_start_date TEXT,
                note TEXT,
                FOREIGN KEY (product_id) REFERENCES tb_product(id)
            );
            ";

        public const string INSERT_PRODUCT_COMPONENT = @"
            INSERT INTO tb_product_component
            (
                product_id,
                name,
                category,
                usage_type,
                usage_value,
                start_type,
                fixed_start_date,
                note
            )
            VALUES
            (
                @ProductId,
                @Name,
                @Category,
                @UsageType,
                @UsageValue,
                @StartType,
                @FixedStartDate,
                @Note
            );
            SELECT last_insert_rowid();
            ";

        public const string DELETE_PRODUCT_COMPONENTS = @"
            DELETE FROM tb_product_component
            WHERE product_id = @ProductId;
            ";

        public const string GET_PRODUCT_COMPONENTS = @"
            SELECT
                id,
                product_id AS ProductId,
                name,
                category AS Category,
                usage_type AS UsageType,
                usage_value AS UsageValue,
                start_type AS StartType,
                fixed_start_date AS FixedStartDate,
                note
            FROM tb_product_component
            WHERE product_id = @ProductId
            ORDER BY id ASC;
            ";
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

        public static string INSERT_PURCHASE = @"
            INSERT INTO tb_purchase
            (
                member_id,
                total_amount,
                discount_amount,
                final_amount,
                payment_method,
                status,
                purchased_at,
                memo,
                created_at,
                updated_at
            )
            VALUES
            (
                @MemberId,
                @TotalAmount,
                @DiscountAmount,
                @FinalAmount,
                @PaymentMethod,
                @Status,
                @PurchasedAt,
                @Memo,
                @CreatedAt,
                @UpdatedAt
            );

            SELECT last_insert_rowid();
        ";

        public static string INSERT_PURCHASE_ITEM = @"
            INSERT INTO tb_purchase_item
            (
                purchase_id,
                product_id,
                product_code_snapshot,
                product_name_snapshot,
                category,
                usage_type,
                start_type,
                unit_price,
                line_amount,
                usage_value,
                is_membership_item,
                note,
                created_at,
                updated_at
            )
            VALUES
            (
                @PurchaseId,
                @ProductId,
                @ProductCodeSnapshot,
                @ProductNameSnapshot,
                @Category,
                @UsageType,
                @StartType,
                @UnitPrice,
                @LineAmount,
                @UsageValue,
                @IsMembershipItem,
                @Note,
                @CreatedAt,
                @UpdatedAt
            );

            SELECT last_insert_rowid();
        ";
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

        public static string INSERT_MEMBER_MEMBERSHIP = @"
            INSERT INTO tb_member_membership
            (
                member_id,
                purchase_id,
                purchase_item_id,
                product_id,
                product_code_snapshot,
                product_name_snapshot,
                category,
                usage_type,
                start_type,
                unit_price_snapshot,
                usage_value,
                duration_days,
                total_count,
                used_count,
                remaining_count,
                purchased_at,
                activated_at,
                start_date,
                end_date,
                status,
                note,
                created_at,
                updated_at
            )
            VALUES
            (
                @MemberId,
                @PurchaseId,
                @PurchaseItemId,
                @ProductId,
                @ProductCodeSnapshot,
                @ProductNameSnapshot,
                @Category,
                @UsageType,
                @StartType,
                @UnitPriceSnapshot,
                @UsageValue,
                @DurationDays,
                @TotalCount,
                @UsedCount,
                @RemainingCount,
                @PurchasedAt,
                @ActivatedAt,
                @StartDate,
                @EndDate,
                @Status,
                @Note,
                @CreatedAt,
                @UpdatedAt
            );

            SELECT last_insert_rowid();
        ";
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

    public static class SqlCodeSequenceQuery
    {

        public const string CREATE_CODE_SEQUENCE_TABLE = @"
            CREATE TABLE IF NOT EXISTS tb_code_sequence (prefix TEXT PRIMARY KEY, last_value INTEGER NOT NULL);";

        public const string INSERT_IF_MISSING = @"
            INSERT OR IGNORE INTO tb_code_sequence(prefix, last_value)
            VALUES (@Prefix, 0);
            ";

        public const string UPDATE_SEQUENCE = @"
            UPDATE tb_code_sequence
            SET last_value = last_value + 1
            WHERE prefix = @Prefix;
            ";

        public const string SELECT_SEQUENCE = @"
            SELECT last_value
            FROM tb_code_sequence
            WHERE prefix = @Prefix;
            ";
    }
}
