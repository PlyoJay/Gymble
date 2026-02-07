using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Gymble.Converter
{
    public class RoundRectClipConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return Geometry.Empty;
            if (values[0] is not double w || values[1] is not double h) return Geometry.Empty;
            if (w <= 0 || h <= 0) return Geometry.Empty;

            double r = 0;
            if (parameter != null) double.TryParse(parameter.ToString(), out r);

            return new RectangleGeometry(new Rect(0, 0, w, h), r, r);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    public class TopRoundRectClipConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return Geometry.Empty;
            if (values[0] is not double w || values[1] is not double h) return Geometry.Empty;
            if (w <= 0 || h <= 0) return Geometry.Empty;

            double r = 0;
            if (parameter != null) double.TryParse(parameter.ToString(), out r);

            // r이 너무 크면 반으로 클램프
            r = Math.Max(0, Math.Min(r, Math.Min(w, h) / 2.0));

            // 상단 좌/우만 둥근 도형
            // (0,r) -> (0,h) -> (w,h) -> (w,r)
            // 상단 오른쪽 arc -> 상단 라인 -> 상단 왼쪽 arc
            var figure = new PathFigure { StartPoint = new Point(0, r), IsClosed = true, IsFilled = true };

            figure.Segments.Add(new LineSegment(new Point(0, h), true));
            figure.Segments.Add(new LineSegment(new Point(w, h), true));
            figure.Segments.Add(new LineSegment(new Point(w, r), true));

            // top-right arc: (w,r) -> (w-r,0)
            figure.Segments.Add(new ArcSegment(
                new Point(w - r, 0),
                new Size(r, r),
                0,
                false,
                SweepDirection.Counterclockwise,
                true));

            // top edge: (w-r,0) -> (r,0)
            figure.Segments.Add(new LineSegment(new Point(r, 0), true));

            // top-left arc: (r,0) -> (0,r)
            figure.Segments.Add(new ArcSegment(
                new Point(0, r),
                new Size(r, r),
                0,
                false,
                SweepDirection.Counterclockwise,
                true));

            return new PathGeometry(new[] { figure });
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
