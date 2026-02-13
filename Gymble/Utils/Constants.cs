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
            public const string StateNormal = "정상";
            public const string StateDormant = "휴면";
            public const string StateSuspended = "정지";
            public const string StateWithdrawn = "탈퇴";
        }
    }
}
