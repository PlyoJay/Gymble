using Gymble.Utils;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Gymble.Converter
{
    public sealed record StatusBadge(string Text, Brush Background, Brush Foreground);

    public sealed class EnumToBadgeConverter : IValueConverter
    {
        private static Brush FromHex(string hex)
            => (Brush)new BrushConverter().ConvertFrom(hex);

        private static readonly Brush White = Brushes.White;

        private static readonly StatusBadge Default =
        new("-", FromHex("#8E8E8E"), White);

        private static readonly Dictionary<Enum, StatusBadge> _map = new()
        {
            // MemberStatus
            { MemberStatus.Active, new StatusBadge(Constants.MemberStatusKor.Active, FromHex("#3EB08C"), White) },
            { MemberStatus.Paused, new StatusBadge(Constants.MemberStatusKor.Paused, FromHex("#F39C12"), White) },
            { MemberStatus.Suspended, new StatusBadge(Constants.MemberStatusKor.Suspended, FromHex("#5C6BC0"), White) },
            { MemberStatus.Expired, new StatusBadge(Constants.MemberStatusKor.Expired, FromHex("#E74C3C"), White) },

            // ProductStatus
            { ProductStatus.OnSale, new StatusBadge(Constants.PublicStatusKor.OnSale, FromHex("#3EB08C"), White) },
            { ProductStatus.Stopped, new StatusBadge(Constants.PublicStatusKor.Stopped, FromHex("#F39C12"), White) },
            { ProductStatus.Discontinued, new StatusBadge(Constants.PublicStatusKor.Discontinued, FromHex("#E74C3C"), White) },
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum e && _map.TryGetValue(e, out var badge))
                return badge;

            return Default;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
