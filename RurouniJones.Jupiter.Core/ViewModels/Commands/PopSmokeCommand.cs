﻿using System;
using System.Diagnostics;
using System.Windows.Input;
using RurouniJones.Jupiter.Core.Models;

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

            Debug.WriteLine($"PopSmokeCommand.Execute called at L/L: {location.Latitude}/{location.Longitude} with color {color}");
        }

        public event EventHandler? CanExecuteChanged;
    }
}