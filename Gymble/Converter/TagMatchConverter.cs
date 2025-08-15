using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Gymble.Converter
{
    public class TagMatchConverter : IValueConverter
    {
        // ViewModel -> View
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && value.ToString() == parameter?.ToString();
        }

        // View -> ViewModel
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isChecked = (bool)value;
            return isChecked ? parameter?.ToString() : Binding.DoNothing;
        }
    }
}
