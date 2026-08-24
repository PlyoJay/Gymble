using System;
using System.Globalization;
using System.Windows.Data;

namespace Gymble.Converter
{
    public class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Equals(parameter?.ToString(), "All", StringComparison.OrdinalIgnoreCase);

            if (parameter == null)
                return false;

            return value.ToString() == parameter.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked)
            {
                var enumType = Nullable.GetUnderlyingType(targetType) ?? targetType;
                var parameterText = parameter?.ToString();

                if (Nullable.GetUnderlyingType(targetType) != null
                    && string.Equals(parameterText, "All", StringComparison.OrdinalIgnoreCase))
                {
                    return null!;
                }

                return Enum.Parse(enumType, parameterText ?? string.Empty);
            }

            return Binding.DoNothing;
        }
    }
}
