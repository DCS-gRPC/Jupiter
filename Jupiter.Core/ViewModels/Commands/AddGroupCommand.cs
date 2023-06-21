using System;
using System.Diagnostics;
using System.Windows.Input;
using Grpc.Net.Client;
using RurouniJones.Dcs.Grpc.V0.Coalition;
using RurouniJones.Dcs.Grpc.V0.Common;
using RurouniJones.Jupiter.Core.Models;

namespace RurouniJones.Jupiter.Core.ViewModels.Commands
{
    public class AddGroupCommand : ICommand
    {
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            if (parameter == null) return;
            var location = (Location) parameter;

            Debug.WriteLine($"AddGroupCommand.Execute called at L/L: {location.Latitude}/{location.Longitude}");

            var rnd = new Random();
            var randomNumber = rnd.Next();

            var template = new AddGroupRequest.Types.GroundGroupTemplate()
            {
                Name = $"SAM Site {randomNumber}",
                Position = new InputPosition()
                {
                    Lat = location.Latitude,
                    Lon = location.Longitude
                },
                Task = "Ground Nothing"
            };

            template.Units.Add(new AddGroupRequest.Types.GroundUnitTemplate() {
                Name = $"SAM Site {randomNumber} Command Post",
                Type = "NASAMS_Command_Post",
                Position = new InputPosition()
                {
                    Lat = location.Latitude,
                    Lon = location.Longitude
                },
                Skill = AddGroupRequest.Types.Skill.Excellent
            });

            template.Units.Add(new AddGroupRequest.Types.GroundUnitTemplate()
            {
                Name = $"SAM Site {randomNumber} Command Post",
                Type = "NASAMS_Radar_MPQ64F1",
                Position = new InputPosition()
                {
                    Lat = location.Latitude - 0.001,
                    Lon = location.Longitude
                },
                Skill = AddGroupRequest.Types.Skill.Excellent
            });

            template.Units.Add(new AddGroupRequest.Types.GroundUnitTemplate()
            {
                Name = $"SAM Site {randomNumber} Launcher 1",
                Type = "NASAMS_LN_C",
                Position = new InputPosition()
                {
                    Lat = location.Latitude + 0.001,
                    Lon = location.Longitude
                },
                Skill = AddGroupRequest.Types.Skill.Excellent
            });

            template.Units.Add(new AddGroupRequest.Types.GroundUnitTemplate()
            {
                Name = $"SAM Site {randomNumber} Launcher 2",
                Type = "NASAMS_LN_C",
                Position = new InputPosition()
                {
                    Lat = location.Latitude + 0.001,
                    Lon = location.Longitude - 0.001
                },
                Skill = AddGroupRequest.Types.Skill.Excellent
            });

            template.Units.Add(new AddGroupRequest.Types.GroundUnitTemplate()
            {
                Name = $"SAM Site {randomNumber} Launcher 3",
                Type = "NASAMS_LN_C",
                Position = new InputPosition()
                {
                    Lat = location.Latitude + 0.001,
                    Lon = location.Longitude + 0.001
                },
                Skill = AddGroupRequest.Types.Skill.Excellent
            });

            try
            {
                using var channel = GrpcChannel.ForAddress($"http://{Global.HostName}:{Global.Port}");
                var client = new CoalitionService.CoalitionServiceClient(channel);
                var result = client.AddGroup(new AddGroupRequest()
                {
                    Country = Country.UnitedStatesOfAmerica,
                    GroupCategory = GroupCategory.Ground,
                    GroundTemplate = template
                }
                );
                Debug.WriteLine($"Created Group Name: {result.Group.Name}");
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        public event EventHandler? CanExecuteChanged;
    }
}
