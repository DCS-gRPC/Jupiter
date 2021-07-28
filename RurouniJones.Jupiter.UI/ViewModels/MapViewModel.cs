using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using MapControl;
using RurouniJones.Jupiter.UI.ViewModels.Commands;

namespace RurouniJones.Jupiter.UI.ViewModels
{
    public class MapViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion INotifyPropertyChanged

        public UIElement CurrentMapLayer => new MapTileLayer
        {
            TileSource = new TileSource {UriFormat = "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"},
            SourceName = "OpenStreetMap",
            Description = "© [OpenStreetMap contributors](http://www.openstreetmap.org/copyright)"
        };

        public PopSmokeCommand PopSmoke { get; }
        public LaunchFlareCommand LaunchFlare { get; }

        private Location _mouseLocation;
        public Location MouseLocation
        {
            get => _mouseLocation;
            set
            {
                _mouseLocation = value;
                OnPropertyChanged("MouseLocation");
            }
        }

        public MapViewModel()
        {
            PopSmoke = new PopSmokeCommand();
            LaunchFlare = new LaunchFlareCommand();
        }
    }
}
