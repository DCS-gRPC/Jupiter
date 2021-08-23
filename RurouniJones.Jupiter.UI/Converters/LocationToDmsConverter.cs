using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using CoordinateSharp;
using RurouniJones.Jupiter.Core.Models;

namespace RurouniJones.Jupiter.UI.Converters
{
    class LocationToDmsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var source = (Location)value;
            var coord = new Coordinate(source.Latitude, source.Longitude);
            var sb = new StringBuilder();
            sb.Append(coord.Latitude.Position.ToString());
            sb.Append(coord.Latitude.Degrees);
            sb.Append("-");
            sb.Append(coord.Latitude.Minutes);
            sb.Append("-");
            sb.Append(coord.Latitude.Seconds);
            sb.Append(" ");
            sb.Append(coord.Longitude.Position.ToString());
            sb.Append(coord.Longitude.Degrees);
            sb.Append("-");
            sb.Append(coord.Longitude.Minutes);
            sb.Append("-");
            sb.Append(coord.Longitude.Seconds);
            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
