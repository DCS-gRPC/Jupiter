using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Geo.Geodesy;
using Grpc.Core;
using Grpc.Net.Client;
using RurouniJones.Dcs.Grpc.V0.Mission;
using RurouniJones.Dcs.FrontLine;
using RurouniJones.Jupiter.Core.Models;
using RurouniJones.Jupiter.Core.ViewModels.Commands;
using SharpVoronoiLib;
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

        private List<CoalitionPolygon> _redForPolygons;
        public List<CoalitionPolygon> RedForPolygons
        {
            get => _redForPolygons;
            set => SetProperty(ref _redForPolygons, value);
        }

        private List<CoalitionPolygon> _blueForPolygons;
        public List<CoalitionPolygon> BlueForPolygons
        {
            get => _blueForPolygons;
            set => SetProperty(ref _blueForPolygons, value);
        }

        public PopSmokeCommand PopSmokeCommand { get; }
        public LaunchFlareCommand LaunchFlareCommand { get; }
        public IlluminationBombCommand IlluminationBombCommand { get; }
        public AddGroupCommand AddGroupCommand { get; }

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
            IlluminationBombCommand = new IlluminationBombCommand();
            AddGroupCommand = new AddGroupCommand();
            Units = new ObservableCollection<Unit>();

            MapLocation = new Location(42, 42);

            var tasks = new List<Task>();
            tasks.Add(StreamUnits()); // TODO Switch to this triggering When we have some sort of "connect" function 
            tasks.Add(StreamEvents()); // maybe using https://stackoverflow.com/questions/11060192/command-to-call-method-from-viewmodel
            tasks.Add(GenerateVoronoi());
        }

        public async Task GenerateVoronoi()
        {
            while(true) {
                try { 
                    await Task.Delay(5000);
                    List <VoronoiSite> sites = new();

                    if(Units.Count == 0) 
                        continue;
                    Debug.WriteLine($"{DateTime.Now}: Generating FrontLine");
                    foreach (Unit unit in Units)
                    {
                        sites.Add(new UnitSite(unit.Location.Longitude, unit.Location.Latitude, (CoalitionId)unit.Coalition));
                    }

                    var generator = new Generator(sites, 36, 40, 46, 46);
                    var polygons = generator.GenerateFrontLines();
                    RedForPolygons = polygons.Where(x => x.Coalition == CoalitionId.RedFor).ToList();
                    BlueForPolygons = polygons.Where(x => x.Coalition == CoalitionId.BlueFor).ToList();

                    Debug.WriteLine($"{DateTime.Now}: Generated FrontLine");
                } catch (Exception ex) {
                    Debug.WriteLine(ex.ToString());
                }
            }
        }

        public async Task StreamUnits()
        {
            try
            {
                using var channel = GrpcChannel.ForAddress($"http://{Global.HostName}:{Global.Port}");
                var client = new MissionService.MissionServiceClient(channel);
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
                        case StreamUnitsResponse.UpdateOneofCase.None:
                            break;
                        case StreamUnitsResponse.UpdateOneofCase.Unit:
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
                                    Pilot = sourceUnit.PlayerName,
                                    Type = sourceUnit.Type,
                                    Player = sourceUnit.PlayerName,
                                    GroupName = sourceUnit.Group.Name,
                                    Callsign = sourceUnit.Callsign,
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
                        case StreamUnitsResponse.UpdateOneofCase.Gone:
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
                var client = new MissionService.MissionServiceClient(channel);
                var events = client.StreamEvents(new StreamEventsRequest());

                //TODO add cancellation token that is triggered on disconnect
                await foreach (var gameEvent in events.ResponseStream.ReadAllAsync())
                {
                    switch (gameEvent.EventCase)
                    {
                        case StreamEventsResponse.EventOneofCase.None:
                            break;
                        case StreamEventsResponse.EventOneofCase.Shot:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.Shot.Initiator.Unit.PlayerName, gameEvent.Shot.Initiator.Unit.Name, gameEvent.ToString()));
                            break;
                        case StreamEventsResponse.EventOneofCase.Hit:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.Hit.Initiator.Unit?.PlayerName, gameEvent.Hit.Initiator.Unit?.Name, gameEvent.ToString()));
                            break;
                        case StreamEventsResponse.EventOneofCase.Takeoff:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.Takeoff.Initiator.Unit?.PlayerName, gameEvent.Takeoff.Initiator.Unit?.Name, gameEvent.ToString()));
                            break;
                        case StreamEventsResponse.EventOneofCase.Land:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.Land.Initiator.Unit?.PlayerName, gameEvent.Land.Initiator.Unit?.Name, gameEvent.ToString()));
                            break;
                        case StreamEventsResponse.EventOneofCase.Crash:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.Crash.Initiator.Unit?.PlayerName, gameEvent.Crash.Initiator.Unit?.Name, gameEvent.ToString()));
                            break;
                        case StreamEventsResponse.EventOneofCase.Ejection:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.Ejection.Initiator.Unit?.PlayerName, gameEvent.Ejection.Initiator.Unit?.Name, gameEvent.ToString()));
                            break;
                        case StreamEventsResponse.EventOneofCase.Refueling:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.Refueling.Initiator.Unit?.PlayerName, gameEvent.Refueling.Initiator.Unit?.Name, gameEvent.ToString()));
                            break;
                        case StreamEventsResponse.EventOneofCase.Dead:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.Dead.Initiator.Unit?.PlayerName, gameEvent.Dead.Initiator.Unit?.Name ?? gameEvent.ToString(), gameEvent.ToString()));
                            break;
                        case StreamEventsResponse.EventOneofCase.PilotDead:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.PilotDead.Initiator.Unit?.PlayerName,  gameEvent.PilotDead.Initiator.Unit?.Name, gameEvent.ToString()));
                            break;
                        case StreamEventsResponse.EventOneofCase.BaseCapture:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.BaseCapture.Initiator.Unit?.PlayerName, gameEvent.BaseCapture.Initiator.Unit?.Name, gameEvent.ToString()));
                            break;
                        case StreamEventsResponse.EventOneofCase.MissionStart:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), null, "MissionSession", gameEvent.ToString()));
                            break;
                        case StreamEventsResponse.EventOneofCase.MissionEnd:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), null, "MissionSession", gameEvent.ToString()));
                            break;
                        case StreamEventsResponse.EventOneofCase.RefuelingStop:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.RefuelingStop.Initiator.Unit?.PlayerName, gameEvent.RefuelingStop.Initiator.Unit?.Name, gameEvent.ToString()));
                            break;
                        case StreamEventsResponse.EventOneofCase.Birth:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.Birth.Initiator.Unit?.PlayerName, gameEvent.Birth.Initiator.Unit?.Name, gameEvent.ToString()));
                            break;
                        case StreamEventsResponse.EventOneofCase.HumanFailure:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.HumanFailure.Initiator.Unit?.PlayerName, gameEvent.HumanFailure.Initiator.Unit?.Name, gameEvent.ToString()));
                            break;
                        case StreamEventsResponse.EventOneofCase.EngineStartup:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.EngineStartup.Initiator.Unit?.PlayerName, gameEvent.EngineStartup.Initiator.Unit?.Name, gameEvent.ToString()));
                            break;
                        case StreamEventsResponse.EventOneofCase.EngineShutdown:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.EngineShutdown.Initiator.Unit?.PlayerName, gameEvent.EngineShutdown.Initiator.Unit?.Name, gameEvent.ToString()));
                            break;
                        case StreamEventsResponse.EventOneofCase.PlayerEnterUnit:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.PlayerEnterUnit.Initiator.Unit?.PlayerName, gameEvent.PlayerEnterUnit.Initiator.Unit?.Name, gameEvent.ToString()));
                            break;
                        case StreamEventsResponse.EventOneofCase.PlayerLeaveUnit:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.PlayerLeaveUnit.Initiator.Unit?.PlayerName, gameEvent.PlayerLeaveUnit.Initiator.Unit?.Name, gameEvent.ToString()));
                            break;
                        case StreamEventsResponse.EventOneofCase.ShootingStart:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.ShootingStart.Initiator.Unit?.PlayerName, gameEvent.ShootingStart.Initiator.Unit?.Name, gameEvent.ToString()));
                            break;
                        case StreamEventsResponse.EventOneofCase.ShootingEnd:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.ShootingEnd.Initiator.Unit?.PlayerName, gameEvent.ShootingEnd.Initiator.Unit?.Name, gameEvent.ToString()));
                            break;
                        case StreamEventsResponse.EventOneofCase.MarkAdd:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.MarkAdd.Initiator?.Unit?.PlayerName, gameEvent.MarkAdd.Initiator?.Unit?.Name, gameEvent.ToString()));
                            break;
                        case StreamEventsResponse.EventOneofCase.MarkChange:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.MarkChange.Initiator?.Unit?.PlayerName, gameEvent.MarkChange.Initiator?.Unit?.Name, gameEvent.ToString()));
                            break;
                        case StreamEventsResponse.EventOneofCase.MarkRemove:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), gameEvent.MarkRemove.Initiator?.Unit?.PlayerName, gameEvent.MarkRemove.Initiator?.Unit?.Name, gameEvent.ToString()));
                            break;
                        default:
                            GameEventCollection.Add(new EventSummary(gameEvent.Time, gameEvent.EventCase.ToString(), "Unknown", "Unknown", "Unknown Event"));
                            break;
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

