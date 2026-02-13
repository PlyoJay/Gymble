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

        public static MemberState ConvertKorToMeberState(string stateKor)
        {
            switch (stateKor)
            {
                default:
                case Constants.MemberStateKor.StateNormal:
                    return MemberState.Normal;
                case Constants.MemberStateKor.StateDormant:
                    return MemberState.Dormant;
                case Constants.MemberStateKor.StateSuspended:
                    return MemberState.Suspended;
                case Constants.MemberStateKor.StateWithdrawn:
                    return MemberState.Withdrawn;
            }
        }

        public static string ConvertMeberStateToKor(MemberState state)
        {
            switch (state)
            {
                default:
                case MemberState.Normal:
                    return Constants.MemberStateKor.StateNormal;
                case MemberState.Dormant:
                    return Constants.MemberStateKor.StateDormant;
                case MemberState.Suspended:
                    return Constants.MemberStateKor.StateSuspended;
                case MemberState.Withdrawn:
                    return Constants.MemberStateKor.StateWithdrawn;
            }
        }
    }
}
