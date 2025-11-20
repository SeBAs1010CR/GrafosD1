using System;

namespace Proyecto.Models
{
    public static class GeoUtils
    {
        private const double R = 6371.0;

        private static double ToRad(double x) => x * Math.PI / 180.0;

        public static double Haversine(double lat1, double lon1, double lat2, double lon2)
        {
            double dLat = ToRad(lat2 - lat1);
            double dLon = ToRad(lon2 - lon1);

            lat1 = ToRad(lat1);
            lat2 = ToRad(lat2);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1) * Math.Cos(lat2) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }
    }
}
