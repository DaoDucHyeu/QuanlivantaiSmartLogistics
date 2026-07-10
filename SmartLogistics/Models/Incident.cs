namespace SmartLogistics.Models
{
    public enum IncidentType
    {
        ChayXe,
        TaiNan,
        HuHong,
        Khac
    }

    public enum IncidentStatus
    {
        DangXuLy,
        DaXuLy
    }

    public class Incident
    {
        public int MaSuCo { get; set; }
        public int MaChuyenDi { get; set; }
        public IncidentType LoaiSuCo { get; set; }
        public string MoTa { get; set; } = string.Empty;
        public string? HinhAnh { get; set; }
        public decimal ViTriLat { get; set; }
        public decimal ViTriLng { get; set; }
        public DateTime ThoiGian { get; set; } = DateTime.Now;
        public IncidentStatus TrangThai { get; set; } = IncidentStatus.DangXuLy;

        // Navigation properties
        public Trip? ChuyenDi { get; set; }
    }
}
