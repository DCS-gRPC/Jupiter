using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Grpc.Core;
using Grpc.Net.Client;
using RurouniJones.Jupiter.Core.Models;
using RurouniJones.Jupiter.Core.ViewModels.Commands;
using RurouniJones.Jupiter.Dcs;
using Coalition = RurouniJones.Jupiter.Core.Models.Coalition;
using Group = RurouniJones.Jupiter.Core.Models.Group;
using Unit = RurouniJones.Jupiter.Core.Models.Unit;

namespace RurouniJones.Jupiter.Core.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<EventSummary> GameEventCollection { get; }
        public ObservableCollection<Unit> Units { get; }
        public ObservableCollection<Coalition> Coalitions { get; }

        public PopSmokeCommand PopSmokeCommand { get; }
        public LaunchFlareCommand LaunchFlareCommand { get; }

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
            Coalitions = Coalition.DefaultCoalitions();
            GameEventCollection = new ObservableCollection<EventSummary>();
            PopSmokeCommand = new PopSmokeCommand();
            LaunchFlareCommand = new LaunchFlareCommand();
            Units = new ObservableCollection<Unit>();
#pragma warning disable 4014
            StreamUnits();  // TODO Switch to this triggering When we have some sort of "connect" function 
            StreamEvents(); // maybe using https://stackoverflow.com/questions/11060192/command-to-call-method-from-viewmodel
            // Create a timer with a two second interval.
            var aTimer = new System.Timers.Timer(10000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += UpdateGroups;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
#pragma warning restore 4014
            MapLocation = new Location(0,0);
        }

                public async Task StreamUnits()
        {
            try
            {
                using var channel = GrpcChannel.ForAddress($"http://{Global.HostName}:{Global.Port}");
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
                                    Location = new Location(sourceUnit.Position.Lat, sourceUnit.Position.Lon, sourceUnit.Position.Alt),
                                    Name = sourceUnit.Name,
                                    Pilot = sourceUnit.Callsign,
                                    Type = sourceUnit.Type
                                };

                                var col = Coalitions.First(c => c.Id == (uint) sourceUnit.Coalition);
                                var grp = col.Groups.FirstOrDefault(g => g.Id == sourceUnit.GroupId);
                                if (grp == null) {
                                    grp = new Group {Id = sourceUnit.GroupId, Name = "New Group", Units = new ObservableCollection<Unit>()};
                                    col.Groups.Add(grp);
                                }
                                grp.Units.Add(newUnit);
                                Units.Add(newUnit);
                                Debug.WriteLine(newUnit);
                            }
                            break;
                        case UnitUpdate.UpdateOneofCase.Gone:
                            var unitDelete = unitUpdate.Gone;
                            var deleted = false;
                            foreach (var coalition in Coalitions)
                            {
                                for (var i = coalition.Groups.Count - 1; i --> 0;)
                                {
                                    var group = coalition.Groups[i];
                                    var unit = group.Units.FirstOrDefault(u => u.Id == unitDelete.Id);
                                    if (unit == null) continue;
                                    group.Units.Remove(unit);
                                    if (group.Units.Count == 0)
                                    {
                                        coalition.Groups.Remove(@group);
                                    }
                                    deleted = true;
                                }
                                if (deleted) break;
                            }
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
                using var channel = GrpcChannel.ForAddress($"http://{Global.HostName}:{Global.Port}");
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

        private void UpdateGroups(object source, ElapsedEventArgs e)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress($"http://{Global.HostName}:{Global.Port}");
                var client = new Dcs.Coalitions.CoalitionsClient(channel);
                var response = client.GetGroups(new GetGroupsRequest());

                foreach (var responseGroup in response.Groups)
                {
                    var updated = false;
                    foreach (var coalition in Coalitions)
                    {
                        foreach (var group in coalition.Groups)
                        {
                            if (group.Id != responseGroup.Id) continue;
                            group.Name = responseGroup.Name;
                            updated = true;
                        }
                        if (updated) break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
    }
}

