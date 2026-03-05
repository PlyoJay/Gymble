using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Gymble.Converter
{
    public sealed class AddUnitConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var unit = parameter?.ToString() ?? string.Empty;

            if (value is null) return string.Empty;

            if (value is string s)
            {
                if (string.IsNullOrWhiteSpace(s)) return string.Empty;
                // 숫자로 못 바꾸면 그대로 + 단위 (원하면 여기서 빈값 처리로 바꿔도 됨)
                if (!decimal.TryParse(s, NumberStyles.Any, culture, out var decFromStr))
                    return s + unit;

                return FormatNumber(decFromStr, culture) + unit;
            }

            try
            {
                var dec = System.Convert.ToDecimal(value, culture);
                return FormatNumber(dec, culture) + unit;
            }
            catch
            {
                return value.ToString() + unit;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;

        private static string FormatNumber(decimal n, CultureInfo culture)
        {
            if (decimal.Truncate(n) == n)
                return n.ToString("N0", culture);
            return n.ToString("N", culture);
        }
    }

    public class UsageValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return "";

            if (values[0] is not ProductUsageType usageType)
                return "";

            if (values[1] == null)
                return "";

            if (!int.TryParse(values[1].ToString(), out var value))
                return "";

            return usageType switch
            {
                ProductUsageType.Period => $"{value}일",
                ProductUsageType.Count => $"{value}회",
                _ => value.ToString()
            };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
