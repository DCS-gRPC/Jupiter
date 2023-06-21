﻿using System;
using System.Diagnostics;
using System.Windows.Input;
using Grpc.Net.Client;
using RurouniJones.Jupiter.Core.Models;
using RurouniJones.Dcs.Grpc.V0.Common;
using RurouniJones.Dcs.Grpc.V0.Trigger;

namespace RurouniJones.Jupiter.Core.ViewModels.Commands
{
    public class PopSmokeCommand : ICommand
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

            Debug.WriteLine($"PopSmokeCommand.Execute called at L/L: {location.Latitude}/{location.Longitude}" +
                            $" with color {color}");
            try
            {
                using var channel = GrpcChannel.ForAddress($"http://{Global.HostName}:{Global.Port}");
                var client = new TriggerService.TriggerServiceClient(channel);
                client.Smoke(new SmokeRequest
                    {
                        Position = new InputPosition
                        {
                            Lat = location.Latitude,
                            Lon = location.Longitude,
                            Alt = 0 // This value is ignored. Smoke is placed at ground level as calculated server-side
                        },
                        Color = (SmokeRequest.Types.SmokeColor) Enum.Parse(
                            typeof(SmokeRequest.Types.SmokeColor), color)
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
