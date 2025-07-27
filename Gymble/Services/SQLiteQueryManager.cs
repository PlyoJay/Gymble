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

        public static string CREATE_PRODUCT_TABLE = "CREATE TABLE IF NOT EXISTS [tb_product] " +
                        "(" +
                            "[id] INTEGER PRIMARY KEY AUTOINCREMENT, " +
                            "[name] TEXT NOT NULL,  " +
                            "[type] TEXT NOT NULL CHECK(type IN ('기간제', '횟수제')),  " +
                            "[duration_days] INTEGER DEFAULT 0,  " +
                            "[total_count] INTEGER DEFAULT 0,  " +
                            "[price] INTEGER NOT NULL  " +
                        ")";
    }

    public class SqlMembershipQuery
    {
        public static string ID = "id";
        public static string MEMBER_ID = "member_id";
        public static string PRODUCT_ID = "product_id";
        public static string PURCHASE_DATE = "purchase_date";
        public static string START_DATE = "start_date";
        public static string EXPIRE_DATE = "expire_date";
        public static string REMAINING_COUNT = "remaining_count";

        public static string CREATE_MEMBERSHIP_TABLE = "CREATE TABLE [tb_membership] " +
                        "(" +
                            "[id] INTEGER PRIMARY KEY AUTOINCREMENT, " +
                            "[member_id] INTEGER NOT NULL,  " +
                            "[product_id] INTEGER NOT NULL,  " +
                            "[purchase_date] TEXT NOT NULL,  " +
                            "[start_date] TEXT NOT NULL,  " +
                            "[expire_date] TEXT,  " +
                            "[remaining_count] INTEGER DEFAULT 0,  " +
                            "FOREIGN KEY (member_id) REFERENCES tb_member(id), " +
                            "FOREIGN KEY (product_id) REFERENCES tb_product(id)" +
                        ")";
    }

    public class SqlAttendanceQuery
    {
        public static string ID = "id";
        public static string MEMBER_ID = "member_id";
        public static string DATETIME = "datetime";

        public static string CREATE_ATTENDANCE_TABLE = "CREATE TABLE [tb_attendance] " +
                        "(" +
                            "[id] INTEGER PRIMARY KEY AUTOINCREMENT, " +
                            "[member_id] INTEGER NOT NULL,  " +
                            "[datetime] TEXT NOT NULL,  " +
                            "FOREIGN KEY (member_id) REFERENCES tb_member(id)" +
                        ")";
    }
}
