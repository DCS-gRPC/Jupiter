using System;
using System.Globalization;

namespace RurouniJones.Jupiter.Core.Models
{
    public class Location : IEquatable<Location>
    {
        public Location(double latitude, double longitude)
        {
            Latitude = Math.Min(Math.Max(latitude, -90.0), 90.0);;
            Longitude = longitude;
        }

        public double Latitude { get; }
        public double Longitude  { get; }

        public bool Equals(Location location)
        {
            return location != null && Math.Abs(location.Latitude - Latitude) < 1E-09 &&
                   Math.Abs(location.Longitude - Longitude) < 1E-09;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Location);
        }

        public override int GetHashCode()
        {
            return Latitude.GetHashCode() ^ Longitude.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:F5},{1:F5}", Latitude, Longitude);
        }
    }
}
