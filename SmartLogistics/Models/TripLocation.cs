namespace SmartLogistics.Models
{
    public class TripLocation
    {
        public int MaToaDo { get; set; }
        public int MaChuyenDi { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal TocDo { get; set; }
        public DateTime ThoiGian { get; set; } = DateTime.Now;

        // Navigation properties
        public Trip? ChuyenDi { get; set; }
    }
}
