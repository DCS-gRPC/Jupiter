using System;
using System.Globalization;
using System.Windows.Data;
using RurouniJones.Jupiter.Core.Models;

namespace RurouniJones.Jupiter.UI.Converters
{
    public class LocationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var source = (Location) value;
            return new MapControl.Location(source.Latitude, source.Longitude);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var source = (MapControl.Location) value;
            return new Location(source.Latitude, source.Longitude);
        }
    }
}
