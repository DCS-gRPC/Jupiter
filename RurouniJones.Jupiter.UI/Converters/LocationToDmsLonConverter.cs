using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using CoordinateSharp;
using RurouniJones.Jupiter.Core.Models;

namespace RurouniJones.Jupiter.UI.Converters
{
    class LocationToDmsLonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var source = (Location)value;
            var coord = new Coordinate(source.Latitude, source.Longitude);
            var sb = new StringBuilder();
            sb.Append(coord.Longitude.Position.ToString());
            sb.Append(coord.Longitude.Degrees);
            sb.Append(" ");
            sb.Append(coord.Longitude.Minutes);
            sb.Append(" ");
            sb.Append(Math.Round(coord.Longitude.Seconds, 4));
            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
