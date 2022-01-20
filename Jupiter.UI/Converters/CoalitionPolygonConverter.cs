using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using RurouniJones.Dcs.FrontLine;
using MapControl;

namespace RurouniJones.Jupiter.UI.Converters
{
    class CoalitionPolygonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var polygon = (CoalitionPolygon)value;
            var locations = polygon.Shell.Coordinates.Select(coord => new Location(coord.Latitude, coord.Longitude)).ToList();
            return new List<List<Location>>() { locations };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
