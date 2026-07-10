using System;

namespace SmartLogistics.Helpers
{
    public static class GeoHelper
    {
        /// <summary>
        /// Tính khoảng cách bằng công thức Haversine cho 2 toạ độ (Vĩ độ, Kinh độ).
        /// Trả về khoảng cách theo đơn vị Kilometers (Km).
        /// </summary>
        public static double GetDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            var R = 6371d; // Bán kính trái đất (km)
            
            var dLat = ToRadians((double)(lat2 - lat1));
            var dLon = ToRadians((double)(lon2 - lon1));
            
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians((double)lat1)) * Math.Cos(ToRadians((double)lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            
            return R * c;
        }

        private static double ToRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }
    }
}
