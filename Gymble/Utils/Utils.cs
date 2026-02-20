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
                case Constants.MemberStateKor.Active:
                    return MemberStatus.Active;
                case Constants.MemberStateKor.Paused:
                    return MemberStatus.Paused;
                case Constants.MemberStateKor.Suspended:
                    return MemberStatus.Suspended;
                case Constants.MemberStateKor.Expired:
                    return MemberStatus.Expired;
            }
        }

        public static string ConvertMeberStateToKor(MemberStatus state)
        {
            switch (state)
            {
                default:
                case MemberStatus.Active:
                    return Constants.MemberStateKor.Active;
                case MemberStatus.Paused:
                    return Constants.MemberStateKor.Paused;
                case MemberStatus.Suspended:
                    return Constants.MemberStateKor.Suspended;
                case MemberStatus.Expired:
                    return Constants.MemberStateKor.Expired;
            }
        }
    }
}
