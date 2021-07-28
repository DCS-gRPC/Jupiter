using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MapControl;
using MapControl.Caching;

namespace RurouniJones.Jupiter.UI
{
    public partial class MainWindow : Window
    {
        public UIElement CurrentMapLayer => new MapTileLayer
        {
            TileSource = new TileSource {UriFormat = "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"},
            SourceName = "OpenStreetMap",
            Description = "© [OpenStreetMap contributors](http://www.openstreetmap.org/copyright)"
        };

        public Location MouseLocation;

        static MainWindow()
        {
            try
            {
                ImageLoader.HttpClient.DefaultRequestHeaders.Add("User-Agent", "DCS.Jupiter");

                TileImageLoader.Cache = new ImageFileCache(TileImageLoader.DefaultCacheFolder);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            DataContext = this;

            if (TileImageLoader.Cache is ImageFileCache cache)
            {
                Loaded += async (s, e) =>
                {
                    await Task.Delay(2000);
                    await cache.Clean();
                };
            }
        }

        private void MainMap_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            MouseLocation = MainMap.ViewToLocation(e.GetPosition(MainMap)); 
            ((ContextMenu) MainMap.FindResource("MapContext")).IsOpen = true;
        }

        private void Flare_OnClick(object sender, RoutedEventArgs e)
        {
            var value = (string) ((MenuItem) sender).Header;
            MessageBox.Show($"Launch {value.ToUpper()} flare at Lat {MouseLocation.Latitude} Lon {MouseLocation.Longitude}");
        }

        private void Smoke_OnClick(object sender, RoutedEventArgs e)
        {
            var value = (string) ((MenuItem) sender).Header;
            MessageBox.Show($"Pop {value.ToUpper()} smoke at Lat {MouseLocation.Latitude} Lon {MouseLocation.Longitude}");
        }
    }
}
