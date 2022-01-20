using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using MapControl;

namespace RurouniJones.Jupiter.UI.Converters
{
    public class LocationsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var source = (ObservableCollection<Core.Models.Location>) value;
            return new LocationCollection(source.Select(location => new MapControl.Location(location.Latitude, location.Longitude)).ToList());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
