using Gymble.Utils;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Gymble.Converter
{
    public sealed record StatusBadge(string Text, Brush Background, Brush Foreground);

    public sealed class MemberStatusToBadgeConverter : IValueConverter
    {
        private static Brush FromHex(string hex)
            => (Brush)new BrushConverter().ConvertFrom(hex);

        private static readonly Brush White = Brushes.White;

        private static readonly StatusBadge Active = new(Constants.MemberStateKor.Active, FromHex("#3EB08C"), White);
        private static readonly StatusBadge Paused = new(Constants.MemberStateKor.Paused, FromHex("#F39C12"), White);
        private static readonly StatusBadge Suspended = new(Constants.MemberStateKor.Suspended, FromHex("#5C6BC0"), White); // 예: 파란/보라 톤
        private static readonly StatusBadge Expired = new(Constants.MemberStateKor.Expired, FromHex("#E74C3C"), White);

        private static readonly StatusBadge Default = new("-", FromHex("#8E8E8E"), White);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Default;

            // enum으로 오면 그대로
            if (value is MemberStatus status)
                return ToBadge(status);

            // DB/바인딩에서 int로 오면 캐스팅
            if (value is int i)
                return ToBadge((MemberStatus)i);

            return Default;
        }

        private static StatusBadge ToBadge(MemberStatus status) => status switch
        {
            MemberStatus.Active => Active,
            MemberStatus.Paused => Paused,
            MemberStatus.Suspended => Suspended,
            MemberStatus.Expired => Expired,
            _ => Default
        };

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
