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
                                var sourceUnit = unitUpdate.Unit;
                                var newUnit = new Unit
                                {
                                    Coalition = (int) sourceUnit.Coalition,
                                    Id = sourceUnit.Id,
                                    Location = new Location(sourceUnit.Position.Lat, sourceUnit.Position.Lon),
                                    Name = sourceUnit.Name,
                                    Pilot = sourceUnit.Callsign,
                                    Type = sourceUnit.Type
                                };
                                Units.Add(newUnit);
                                Debug.WriteLine(newUnit);
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
