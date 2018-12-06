using System;

namespace Tavisca.Platform.Common.Logging
{
    [Serializable]
    public class GeoPoint : IEquatable<GeoPoint>
    {
        public GeoPoint(decimal latitude, decimal longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public decimal Latitude { get; }

        public decimal Longitude { get; }

        public override bool Equals(object obj)
        {
            var other = obj as GeoPoint;
            if (other == null)
                return false;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return Latitude.GetHashCode() ^ Longitude.GetHashCode();
        }

        public bool Equals(GeoPoint other)
        {
            if (other == null)
                return false;
            return Latitude.Equals(other.Latitude) && Longitude.Equals(other.Longitude);
        }
    }

}
