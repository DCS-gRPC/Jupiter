using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RurouniJones.Jupiter.UI.Converters
{
    public class LocationAndColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == DependencyProperty.UnsetValue) return false;
            return ((Core.Models.Location) values[0], (string) values[1]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
