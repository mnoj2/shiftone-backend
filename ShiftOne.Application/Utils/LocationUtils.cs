using System;

namespace ShiftOne.Application.Utils
{
    public static class LocationUtils
    {
        public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var r = 6371; // radius of earth in km
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return r * c * 1000; // distance in meters
        }

        private static double ToRadians(double angle) => Math.PI * angle / 180.0;
    }
}
