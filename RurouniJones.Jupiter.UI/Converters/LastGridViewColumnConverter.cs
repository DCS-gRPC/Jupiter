using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace RurouniJones.Jupiter.UI.Converters
{
    public class LastGridViewColumnConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var listView = values[1] as ListView;
            var width = listView.ActualWidth;
            var gv = listView.View as GridView;
            for (var i = 0; i < gv.Columns.Count - 1; i++)
            {
                if (!double.IsNaN(gv.Columns[i].Width))
                    width -= gv.Columns[i].Width;
            }

            width -= 21; // Scrollbar
            
            return width - 5; // this is to take care of margin/padding 
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
