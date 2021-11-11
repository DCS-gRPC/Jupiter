using System;
using System.Diagnostics;
using System.Windows.Input;
using Grpc.Net.Client;
using RurouniJones.Jupiter.Core.Models;
using RurouniJones.Jupiter.Dcs.Common;
using RurouniJones.Jupiter.Dcs.Trigger;

namespace RurouniJones.Jupiter.Core.ViewModels.Commands
{
    public class IlluminationBombCommand : ICommand
    {
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            if (parameter == null) return;
            var location = ((ValueTuple<Location, string>) parameter).Item1;
            var color = ((ValueTuple<Location, string>) parameter).Item2;

            Debug.WriteLine($"IlluminationBomb.Execute called at L/L: {location.Latitude}/{location.Longitude}" +
                            $"with color {color}");
            try
            {
                using var channel = GrpcChannel.ForAddress($"http://{Global.HostName}:{Global.Port}");
                var client = new TriggerService.TriggerServiceClient(channel);
                client.IlluminationBomb(new IlluminationBombRequest
                    {
                        Position = new Position
                        {
                            Lat = location.Latitude,
                            Lon = location.Longitude,
                            Alt = 1066
                        },
                    }
                );
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        public event EventHandler? CanExecuteChanged;
    }
}
