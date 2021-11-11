using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using CoordinateSharp;
using RurouniJones.Jupiter.Core.Models;

namespace RurouniJones.Jupiter.UI.Converters
{
    class LocationToDmsLatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var source = (Location)value;
            var coord = new Coordinate(source.Latitude, source.Longitude);
            var sb = new StringBuilder();
            sb.Append(coord.Latitude.Position.ToString());
            sb.Append(coord.Latitude.Degrees);
            sb.Append(" ");
            sb.Append(coord.Latitude.Minutes);
            sb.Append(" ");
            sb.Append(Math.Round(coord.Latitude.Seconds, 4));
            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
