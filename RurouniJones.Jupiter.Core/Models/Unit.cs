using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;

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

                var attributes = new List<string> {"fixed wing", "fighter"};

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
                sb.Append(".png");

                var basePath = AppDomain.CurrentDomain.BaseDirectory;
                var imagePath = Path.Combine(basePath, $"Assets\\MapIcons\\{sb}");
                return File.Exists(imagePath) ? imagePath : GetGenericSymbol();
            }
        }

        private string GetGenericSymbol()
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
            sb.Append(Type.Split("+")[0]);

            sb.Append(".png");

            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var imagePath = Path.Combine(basePath, $"Assets\\MapIcons\\{sb}");

            return File.Exists(imagePath) ? imagePath : Path.Combine(basePath, "Assets\\MapIcons\\Unknown.png");
        }
    }
}
