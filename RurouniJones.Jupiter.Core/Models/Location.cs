using System;
using System.Globalization;

namespace RurouniJones.Jupiter.Core.Models
{
    public class Location : IEquatable<Location>
    {
        private double _latitude;
        private double _longitude;

        public Location()
        {
        }

        public Location(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public double Latitude
        {
            get => _latitude;
            set => _latitude = Math.Min(Math.Max(value, -90.0), 90.0);
        }

        public double Longitude
        {
            get => _longitude;
            set => _longitude = value;
        }

        public bool Equals(Location location)
        {
            return location != null && Math.Abs(location._latitude - _latitude) < 1E-09 &&
                   Math.Abs(location._longitude - _longitude) < 1E-09;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Location);
        }

        public override int GetHashCode()
        {
            return _latitude.GetHashCode() ^ _longitude.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:F5},{1:F5}", _latitude, _longitude);
        }
    }
}
