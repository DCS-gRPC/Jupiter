using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using RurouniJones.Jupiter.Core.Models;
using RurouniJones.Jupiter.Dcs;

namespace RurouniJones.Jupiter.Core.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<EventSummary> GameEventCollection { get; }

        public MainViewModel()
        {
            GameEventCollection = new ObservableCollection<EventSummary>();
#pragma warning disable 4014
            StreamEvents(); /* TODO Switch to this triggering When we have some sort of "connect" function 
                             * maybe using https://stackoverflow.com/questions/11060192/command-to-call-method-from-viewmodel
                             */
#pragma warning restore 4014
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

