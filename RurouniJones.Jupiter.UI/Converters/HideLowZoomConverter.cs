using System;
using System.Globalization;
using System.Windows.Data;

namespace RurouniJones.Jupiter.UI.Converters
{
    public class HideLowZoomConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var zoomLevel = (int) value;
            return zoomLevel  < 11 ? System.Windows.Visibility.Hidden : System.Windows.Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
