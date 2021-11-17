using System;
using System.Diagnostics;
using System.Windows.Input;
using Grpc.Net.Client;

namespace RurouniJones.Jupiter.Core.ViewModels.Commands
{
    public class DisableRadarEmissionCommand : ICommand
    {
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            if (parameter == null) return;
            var unitName = (string) parameter;

            Debug.WriteLine($"DisableRadarEmission.Execute called for unit '{unitName}'");
            try
            {
                using var channel = GrpcChannel.ForAddress($"http://{Global.HostName}:{Global.Port}");
                var client = new Dcs.Unit.UnitService.UnitServiceClient(channel);
                client.SetEmissionAsync(new Dcs.Unit.SetEmissionRequest()
                    {
                        Name = unitName,
                        Emitting = false
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