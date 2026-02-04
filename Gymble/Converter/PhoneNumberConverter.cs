using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Gymble.Converter
{
    class PhoneNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string phone)
            {
                return $"{phone.Substring(0,3)}-{phone.Substring(3, 4)}-{phone.Substring(7, 4)}";
            }
            return string.Empty ;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string phone && phone.Contains("-"))
            {
                var str = $"{phone.Split('-')[0]}{phone.Split('-')[1]}{phone.Split('-')[2]}";
                return str;
            }
            return string.Empty;
        }
    }
}
