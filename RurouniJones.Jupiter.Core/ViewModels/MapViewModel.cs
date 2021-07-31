using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using RurouniJones.Jupiter.Core.Models;
using RurouniJones.Jupiter.Core.ViewModels.Commands;
using RurouniJones.Jupiter.Dcs;
using Unit = RurouniJones.Jupiter.Core.Models.Unit;

namespace RurouniJones.Jupiter.Core.ViewModels
{
    public class MapViewModel : ViewModelBase
    {
        public PopSmokeCommand PopSmoke { get; }
        public LaunchFlareCommand LaunchFlare { get; }

        public ObservableCollection<Unit> Units { get; }

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
            Units = new ObservableCollection<Unit>();
            StreamUnits();
        }

        public async Task StreamUnits()
        {
            try
            {
                using var channel = GrpcChannel.ForAddress($"http://127.0.0.1:50051");
                var client = new Mission.MissionClient(channel);
                var units = client.StreamUnits(new StreamUnitsRequest()
                {
                    PollRate = 1,
                    MaxBackoff = 30
                });

                //TODO add cancellation token that is triggered on disconnect
                await foreach (var unitUpdate in units.ResponseStream.ReadAllAsync())
                {
                    switch(unitUpdate.UpdateCase)
                    {
                        case UnitUpdate.UpdateOneofCase.None:
                            break;
                        case UnitUpdate.UpdateOneofCase.Unit:
                            if (Units.Any(u => u.Id == unitUpdate.Unit.Id))
                            {
                                var unitDetails = unitUpdate.Unit;
                                Units.First(u => u.Id == unitDetails.Id).Location = new Location(unitDetails.Position.Lat, unitDetails.Position.Lon);
                            }
                            else
                            {
                                var newUnit = unitUpdate.Unit;
                                Units.Add(new Unit()
                                {
                                    Coalition = (int) newUnit.Coalition,
                                    Id = newUnit.Id,
                                    Location = new Location(newUnit.Position.Lat, newUnit.Position.Lon),
                                    Name = newUnit.Name,
                                    Pilot = newUnit.Callsign,
                                    Type = newUnit.Type
                                });
                            }
                            break;
                        case UnitUpdate.UpdateOneofCase.Gone:
                            var unitDelete = unitUpdate.Gone;
                            Units.Remove(Units.First(u => u.Id == unitDelete.Id));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }
    }
}
