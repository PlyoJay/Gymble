using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gymble.Utils
{
    class Constants
    {
        public class Database
        {
            public const string FolderName = "Database";
            public const string FileName = "database.db";
        }

        public class MemberStateKor
        {
            public const string Active = "사용중";
            public const string Paused = "일시정지";
            public const string Suspended = "정지";
            public const string Expired = "만료";
        }
    }
}
