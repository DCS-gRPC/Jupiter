using System;
using System.Diagnostics;
using System.Windows.Input;
using Grpc.Net.Client;
using RurouniJones.Jupiter.Dcs;

namespace RurouniJones.Jupiter.Core.ViewModels.Commands
{
    public class EnableRadarEmissionCommand : ICommand
    {
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            if (parameter == null) return;
            var unitName = (string) parameter;

            Debug.WriteLine($"EnableRadarEmission.Execute called for unit '{unitName}'");
            try
            {
                using var channel = GrpcChannel.ForAddress($"http://127.0.0.1:50051");
                var client = new Units.UnitsClient(channel);
                client.EnableEmission(new EnableEmissionRequest
                    {
                        Name = unitName
                        // Enabled = true
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