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

        private int _zoomLevel = 7;
        public int ZoomLevel
        {
            get => _zoomLevel;
            set => SetProperty(ref _zoomLevel, value);
        }

        private Location _mouseLocation;
        public Location MouseLocation
        {
            get => _mouseLocation;
            set => SetProperty(ref _mouseLocation, value);
        }

        private Unit _selectedUnit;
        public Unit SelectedUnit
        {
            get => _selectedUnit;
            set => SetProperty(ref _selectedUnit, value);
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
                                Units.First(u => u.Id == unitDetails.Id).Location = new Location(unitDetails.Position.Lat, unitDetails.Position.Lon, unitDetails.Position.Alt);
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
                                    Type = sourceUnit.Type,
                                    Player = sourceUnit.PlayerName,
                                    GroupName = sourceUnit.GroupName,
                                    MilStd2525dCode = Encyclopedia.Repository.GetMilStd2525DCodeByDcsCode(sourceUnit.Type),
                                };
                                Units.Add(newUnit);

                                var col = Coalitions.First(c => c.Id == (uint) newUnit.Coalition);
                                var grp = col.Groups.FirstOrDefault(g => g.Name == newUnit.GroupName);
                                if (grp != null)
                                {
                                    grp.Units.Add(newUnit);
                                }
                                else
                                {
                                    grp = new Group
                                    {
                                        Name = newUnit.GroupName,
                                        Units = new ObservableCollection<Unit> { newUnit }
                                    };
                                    col.Groups.Add(grp);
                                }

                                Debug.WriteLine($"Added {newUnit}");
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
                                    var unit = group.Units.FirstOrDefault(u => u.Name == unitDelete.Name);
                                    if (unit == null)
                                    {
                                        Debug.WriteLine($"Unit \"{unitDelete.Name}\" not in group \"{group.Name}\"");
                                        continue;
                                    }
                                    Debug.WriteLine($"Found Unit \"{unit.Name}\" in group \"{group.Name}\"");
                                    if (group.Units.Count <= 1)
                                    {
                                        Debug.WriteLine($"Deleted Group \"{group.Name}\"");
                                        coalition.Groups.Remove(group);
                                    }
                                    Debug.WriteLine($"Deleted Unit \"{unit.Name}\" from group \"{group.Name}\"");
                                    group.Units.Remove(unit);
                                    deleted = true;
                                }
                                if (deleted) break;
                                Debug.WriteLine($"Unit \"{unitDelete.Name}\" to be deleted but not found in Coalition \"{coalition.Name}\"");
                            }
                            var unitToDelete = Units.FirstOrDefault(u => u.Name == unitDelete.Name);
                            if (unitToDelete == null)
                            {
                                Debug.WriteLine($"Unit \"{unitDelete.Name}\" is to be deleted but could not be found");
                            }
                            else
                            {
                                Debug.WriteLine($"Deleted Unit \"{unitDelete.Name}\"");
                                Units.Remove(unitToDelete);
                            }
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
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.Shot.Initiator.Unit.PlayerName, gameEvent.Shot.Initiator.Unit.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.Hit:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.Hit.Initiator.Unit?.PlayerName, gameEvent.Hit.Initiator.Unit?.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.Takeoff:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.Takeoff.Initiator.Unit?.PlayerName, gameEvent.Takeoff.Initiator.Unit?.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.Land:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.Land.Initiator.Unit?.PlayerName, gameEvent.Land.Initiator.Unit?.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.Crash:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.Crash.Initiator.Unit?.PlayerName, gameEvent.Crash.Initiator.Unit?.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.Ejection:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.Ejection.Initiator.Unit?.PlayerName, gameEvent.Ejection.Initiator.Unit?.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.Refueling:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.Refueling.Initiator.Unit?.PlayerName, gameEvent.Refueling.Initiator.Unit?.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.Dead:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.Dead.Initiator.Unit?.PlayerName, gameEvent.Dead.Initiator.Unit?.Name ?? gameEvent.ToString(), gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.PilotDead:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.PilotDead.Initiator.Unit?.PlayerName,  gameEvent.PilotDead.Initiator.Unit?.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.BaseCapture:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.BaseCapture.Initiator.Unit?.PlayerName, gameEvent.BaseCapture.Initiator.Unit?.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.MissionStart:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), null, "MissionSession", gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.MissionEnd:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), null, "MissionSession", gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.RefuelingStop:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.RefuelingStop.Initiator.Unit?.PlayerName, gameEvent.RefuelingStop.Initiator.Unit?.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.Birth:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.Birth.Initiator.Unit?.PlayerName, gameEvent.Birth.Initiator.Unit?.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.SystemFailure:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.SystemFailure.Initiator.Unit?.PlayerName, gameEvent.SystemFailure.Initiator.Unit?.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.EngineStartup:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.EngineStartup.Initiator.Unit?.PlayerName, gameEvent.EngineStartup.Initiator.Unit?.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.EngineShutdown:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.EngineShutdown.Initiator.Unit?.PlayerName, gameEvent.EngineShutdown.Initiator.Unit?.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.PlayerEnterUnit:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.PlayerEnterUnit.Initiator.Unit?.PlayerName, gameEvent.PlayerEnterUnit.Initiator.Unit?.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.PlayerLeaveUnit:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.PlayerLeaveUnit.Initiator.Unit?.PlayerName, gameEvent.PlayerLeaveUnit.Initiator.Unit?.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.ShootingStart:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.ShootingStart.Initiator.Unit?.PlayerName, gameEvent.ShootingStart.Initiator.Unit?.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.ShootingEnd:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.ShootingEnd.Initiator.Unit?.PlayerName, gameEvent.ShootingEnd.Initiator.Unit?.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.MarkAdd:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.MarkAdd.Initiator?.Unit?.PlayerName, gameEvent.MarkAdd.Initiator?.Unit?.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.MarkChange:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.MarkChange.Initiator?.Unit?.PlayerName, gameEvent.MarkChange.Initiator?.Unit?.Name, gameEvent.ToString()));
                            break;
                        case Event.EventOneofCase.MarkRemove:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.MarkRemove.Initiator?.Unit?.PlayerName, gameEvent.MarkRemove.Initiator?.Unit?.Name, gameEvent.ToString()));
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

