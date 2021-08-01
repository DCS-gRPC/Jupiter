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
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<EventSummary> GameEventCollection { get; }
        public ObservableCollection<Unit> Units { get; }

        public PopSmokeCommand PopSmoke { get; }
        public LaunchFlareCommand LaunchFlare { get; }

        private Location _mapLocation;
        public Location MapLocation
        {
            get => _mapLocation;
            set => SetProperty(ref _mapLocation, value);
        }

        private Location _mouseLocation;
        public Location MouseLocation
        {
            get => _mouseLocation;
            set => SetProperty(ref _mouseLocation, value);
        }

        public MainViewModel()
        {
            GameEventCollection = new ObservableCollection<EventSummary>();
            PopSmoke = new PopSmokeCommand();
            LaunchFlare = new LaunchFlareCommand();
            Units = new ObservableCollection<Unit>();
#pragma warning disable 4014
            StreamUnits();  // TODO Switch to this triggering When we have some sort of "connect" function 
            StreamEvents(); // maybe using https://stackoverflow.com/questions/11060192/command-to-call-method-from-viewmodel
#pragma warning restore 4014
            MapLocation = new Location(0,0);
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

        public async Task StreamEvents()
        {
            try
            {
                using var channel = GrpcChannel.ForAddress($"http://127.0.0.1:50051");
                var client = new Mission.MissionClient(channel);
                var events = client.StreamEvents(new StreamEventsRequest());

                //TODO add cancellation token that is triggered on disconnect
                await foreach (var gameEvent in events.ResponseStream.ReadAllAsync())
                {
                    switch (gameEvent.EventCase)
                    {
                        case Event.EventOneofCase.None:
                            break;
                        case Event.EventOneofCase.Shot:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.Shot.Initiator.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.Hit:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.Hit.Initiator.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.Takeoff:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.Takeoff.Initiator.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.Land:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.Land.Initiator.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.Crash:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.Crash.Initiator.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.Ejection:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.Ejection.Initiator.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.Refueling:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.Refueling.Initiator.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.Dead:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.Dead.Name ?? gameEvent.Dead.Id.ToString(), gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.PilotDead:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.PilotDead.Initiator.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.BaseCapture:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.BaseCapture.Initiator.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.MissionStart:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), "MissionSession", gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.MissionEnd:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), "MissionSession", gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.RefuelingStop:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.RefuelingStop.Initiator.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.Birth:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.Birth.Initiator.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.SystemFailure:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.SystemFailure.Initiator.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.EngineStartup:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.EngineStartup.Initiator.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.EngineShutdown:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.EngineShutdown.Initiator.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.PlayerEnterUnit:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.PlayerEnterUnit.Initiator.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.PlayerLeaveUnit:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.PlayerLeaveUnit.Initiator.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.ShootingStart:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.ShootingStart.Initiator.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.ShootingEnd:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.ShootingEnd.Initiator.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.MarkAdd:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.MarkAdd.Initiator.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.MarkChange:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.MarkChange.Initiator.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.MarkRemove:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.MarkRemove.Initiator.Name, gameEvent.ToString()));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    Console.WriteLine(gameEvent.ToString());
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }
    }
}

