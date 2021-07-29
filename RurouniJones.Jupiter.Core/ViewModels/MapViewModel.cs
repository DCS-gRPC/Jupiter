using RurouniJones.Jupiter.Core.Models;
using RurouniJones.Jupiter.Core.ViewModels.Commands;

namespace RurouniJones.Jupiter.Core.ViewModels
{
    public class MapViewModel : ViewModelBase
    {
        public PopSmokeCommand PopSmoke { get; }
        public LaunchFlareCommand LaunchFlare { get; }

        private Location _mouseLocation;
        public Location MouseLocation
        {
            get => _mouseLocation;
            set => SetProperty(ref _mouseLocation, value);
        }

        public MapViewModel()
        {
            PopSmoke = new PopSmokeCommand();
            LaunchFlare = new LaunchFlareCommand();
        }
    }
}
