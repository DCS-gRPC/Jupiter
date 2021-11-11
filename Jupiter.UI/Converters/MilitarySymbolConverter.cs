using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Data;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;
using Jint;
using RurouniJones.Jupiter.Core.Models;

namespace RurouniJones.Jupiter.UI.Converters
{
    class MilitarySymbolConverter : IValueConverter
    {
        private static readonly string BasePath = AppDomain.CurrentDomain.BaseDirectory;
        private static Engine _jsEngine; 

        private static void InitJsEngine()
        {
            var milSymbol = File.ReadAllText(Path.Combine(BasePath, $"Assets\\milsymbol.min.js"));
            _jsEngine = new Engine().Execute(milSymbol);
        }
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ulong code = 10001000000000000000;
            if (value != null)
            {
                var unit = (Unit) value;
                code = unit.MilStd2525dCode;
                code = unit.Coalition switch
                {
                    0 => code += 40000000000000000,
                    1 => code += 60000000000000000,
                    2 => code += 30000000000000000
                };
            }

            var pngImagePath = Path.Combine(BasePath, $"Assets\\MapIcons\\{code}.png");

            if (File.Exists(pngImagePath)) return pngImagePath;

            if (_jsEngine == null)
            {
                InitJsEngine();
            }

            var svgString = _jsEngine.Evaluate($"new ms.Symbol(\"{code}\", {{ size: 50 }}).asSVG();").ToString();
            var svgStream = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(svgString)));

            var settings = new WpfDrawingSettings();
            var converter = new ImageSvgConverter(settings)
            {
                EncoderType = ImageEncoderType.PngBitmap
            };

            converter.Convert(svgStream, pngImagePath);

            return pngImagePath;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
