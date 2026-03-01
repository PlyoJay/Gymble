using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Gymble.Converter
{
    public class RowHeightConverter : IValueConverter
    {
        public double HeaderHeight { get; set; } = 35;   // 헤더 높이
        public int RowCount { get; set; } = 15;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double totalHeight && totalHeight > 0)
            {
                double line = 1; // Grid line 1px 가정
                double available = totalHeight - HeaderHeight - (RowCount * line);
                return available / RowCount;
            }

            return 25; // 기본값
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
