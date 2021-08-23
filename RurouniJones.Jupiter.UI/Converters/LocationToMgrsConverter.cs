using System;
using System.Globalization;
using System.Windows.Data;
using CoordinateSharp;
using RurouniJones.Jupiter.Core.Models;

namespace RurouniJones.Jupiter.UI.Converters
{
    class LocationToMgrsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var source = (Location)value;

            return new Coordinate(source.Latitude, source.Longitude).MGRS.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
