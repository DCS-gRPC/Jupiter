using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using RurouniJones.Jupiter.Encyclopedia.Repositories;

namespace RurouniJones.Jupiter.Core.Models
{
    public class Unit : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private Location _location;
        public Location Location
        {
            get => _location;
            set
            {
                _location = value;
                RaisePropertyChanged(nameof(Location));
            }
        }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                RaisePropertyChanged(nameof(Name));
            }
        }

        private uint _id;
        public uint Id
        {
            get => _id;
            set
            {
                _id = value;
                RaisePropertyChanged(nameof(Id));
            }
        }

        private int _coalition;
        public int Coalition
        {
            get => _coalition;
            set
            {
                _coalition = value;
                RaisePropertyChanged(nameof(Coalition));
            }
        }

        private string _pilot;
        public string Pilot
        {
            get => _pilot;
            set
            {
                _pilot = value;
                RaisePropertyChanged(nameof(Pilot));
            }
        }

        private string _type;
        public string Type
        {
            get => _type;
            set
            {
                _type = value;
                RaisePropertyChanged(nameof(Type));
            }
        }
        
        private readonly string _basePath = AppDomain.CurrentDomain.BaseDirectory;

        // This needs to be moved out of Core and into UI. The core model should only have the
        // attributes. It is the UI project's job to turn that into an image path.
        public string Image {
            get
            {
                var sb = new StringBuilder();
                switch (Coalition)
                {
                    case 0:
                        sb.Append("Neutral");
                        break;
                    case 1:
                        sb.Append("Red");
                        break;
                    case 2:
                        sb.Append("Blue");
                        break;
                }

                var vehicle = GetVehicleIcon(Type, sb);
                if(vehicle != null)
                    return Path.Combine(_basePath, $"Assets\\MapIcons\\{vehicle}.png");

                var aircraft = GetAircraftIcon(Type, sb);
                if(aircraft != null)
                    return Path.Combine(_basePath, $"Assets\\MapIcons\\{aircraft}.png");

                var watercraft = GetWatercraftIcon(Type, sb);
                if(watercraft != null)
                    return Path.Combine(_basePath, $"Assets\\MapIcons\\{watercraft}.png");

                return Path.Combine(_basePath, "Assets\\MapIcons\\Unknown.png");
            }
        }

        private static string GetWatercraftIcon(string type, StringBuilder sb)
        {
            var attributes = WatercraftRepository.GetAttributesByDcsCode(type);

            if (attributes.Count == 0) return null;

            if (attributes.Contains("aircraft carrier"))
                sb.Append("AircraftCarrier");
            else if (attributes.Contains("cruiser"))
                sb.Append("Cruiser");

            return sb.ToString();
        }

        private static string GetVehicleIcon(string type, StringBuilder sb)
        {
            var attributes = VehicleRepository.GetAttributesByDcsCode(type);

            if (attributes.Count == 0) return null;

            if (attributes.Contains("mechanised"))
                sb.Append("Mechanised");
            else if (attributes.Contains("motorised"))
                sb.Append("Motorised");
            else if (attributes.Contains("wheeled"))
                sb.Append("Wheeled");

            if (attributes.Contains("infantry carrier"))
                sb.Append("InfantryCarrier");
            else if (attributes.Contains("aaa"))
                sb.Append("AAA");

            return sb.ToString();
        }


        private static string GetAircraftIcon(string type, StringBuilder sb)
        {
            var attributes = AircraftRepository.GetAttributesByDcsCode(type);

            if (attributes.Count == 0) return null;

            if (attributes.Contains("fixed wing"))
                sb.Append("FixedWing");
            else if (attributes.Contains("rotary"))
                sb.Append("Rotary");

            if (attributes.Contains("fighter"))
                sb.Append("Fighter");
            else if (attributes.Contains("attack"))
                sb.Append("Attacker");
            else if (attributes.Contains("tanker"))
                sb.Append("Tanker");
            else if (attributes.Contains("awacs"))
                sb.Append("AEW");

            return sb.ToString();
        }

        public override string ToString()
        {
            return $"{nameof(Location)}: {Location}, {nameof(Name)}: {Name}, {nameof(Id)}: {Id}, {nameof(Coalition)}: {Coalition}, {nameof(Pilot)}: {Pilot}, {nameof(Type)}: {Type}, {nameof(Image)}: {Image}";
        }
    }
}
