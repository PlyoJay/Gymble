using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gymble.Utils
{
    public static class Utils
    {
        public static readonly string CurrentDirectory = Environment.CurrentDirectory;

        public static MemberStatus ConvertKorToMeberState(string stateKor)
        {
            switch (stateKor)
            {
                default:
                case Constants.MemberStatusKor.Active:
                    return MemberStatus.Active;
                case Constants.MemberStatusKor.Paused:
                    return MemberStatus.Paused;
                case Constants.MemberStatusKor.Suspended:
                    return MemberStatus.Suspended;
                case Constants.MemberStatusKor.Expired:
                    return MemberStatus.Expired;
            }
        }

        public static string ConvertMeberStateToKor(MemberStatus state)
        {
            switch (state)
            {
                default:
                case MemberStatus.Active:
                    return Constants.MemberStatusKor.Active;
                case MemberStatus.Paused:
                    return Constants.MemberStatusKor.Paused;
                case MemberStatus.Suspended:
                    return Constants.MemberStatusKor.Suspended;
                case MemberStatus.Expired:
                    return Constants.MemberStatusKor.Expired;
            }
        }
    }
}
