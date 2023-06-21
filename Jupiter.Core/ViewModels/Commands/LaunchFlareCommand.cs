﻿using System;
using System.Diagnostics;
using System.Windows.Input;
using Grpc.Net.Client;
using RurouniJones.Jupiter.Core.Models;
using RurouniJones.Dcs.Grpc.V0.Common;
using RurouniJones.Dcs.Grpc.V0.Trigger;

namespace RurouniJones.Jupiter.Core.ViewModels.Commands
{
    public class LaunchFlareCommand : ICommand
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

            Debug.WriteLine($"LaunchFlareCommand.Execute called at L/L: {location.Latitude}/{location.Longitude}" +
                            $"with color {color}");
            try
            {
                using var channel = GrpcChannel.ForAddress($"http://{Global.HostName}:{Global.Port}");
                var client = new TriggerService.TriggerServiceClient(channel);
                client.SignalFlare(new SignalFlareRequest
                    {
                        Position = new InputPosition
                        {
                            Lat = location.Latitude,
                            Lon = location.Longitude,
                            Alt = location.Altitude
                        },
                        Color = (SignalFlareRequest.Types.FlareColor) Enum.Parse(
                            typeof(SignalFlareRequest.Types.FlareColor), color)
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
