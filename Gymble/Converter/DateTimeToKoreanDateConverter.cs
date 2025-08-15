using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Gymble.Converter
{
    public class DateTimeToKoreanDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                return dateTime.ToString("yyyy년 MM월 dd일", CultureInfo.InvariantCulture);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str &&
                DateTime.TryParseExact(str, "yyyy년 MM월 dd일",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            {
                return result;
            }
            return DateTime.MinValue;
        }
    }
}
