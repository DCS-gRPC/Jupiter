using System;
using System.Diagnostics;
using System.Windows.Input;
using MapControl;

namespace RurouniJones.Jupiter.UI.ViewModels.Commands
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

            Debug.WriteLine($"LaunchFlareCommand.Execute called at L/L: {location.Latitude}/{location.Longitude} with color {color}");
        }

        public event EventHandler? CanExecuteChanged;
    }
}
