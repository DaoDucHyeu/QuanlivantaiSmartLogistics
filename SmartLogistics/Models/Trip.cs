namespace SmartLogistics.Models
{
    public enum TripStatus
    {
        ChuaBatDau,
        DangThucHien,
        HoanThanh,
        Huy
    }

    public class Trip
    {
        public int MaChuyenDi { get; set; }
        public int MaXe { get; set; }
        public string TaiXeId { get; set; } = string.Empty;
        public DateTime ThoiGianBatDau { get; set; }
        public DateTime? ThoiGianKetThuc { get; set; }
        public TripStatus TrangThai { get; set; } = TripStatus.ChuaBatDau;
        public decimal TongKhoangCach { get; set; }
        public string? GhiChu { get; set; }

        // Navigation properties
        public Vehicle? Xe { get; set; }
        public TaiXe? TaiXe { get; set; }
        public ICollection<TripDetail> TripDetails { get; set; } = new List<TripDetail>();
        public ICollection<Incident> Incidents { get; set; } = new List<Incident>();
        public ICollection<TripLocation> TripLocations { get; set; } = new List<TripLocation>();

        // Mối quan hệ với Điều hành tạo/quản lý chuyến đi này
        public string? DieuHanhId { get; set; }
        public virtual DieuHanh? DieuHanh { get; set; }

        // Multi-Tenant: Thuộc về Admin (Công ty) nào
        public string? AdminId { get; set; }
        public virtual Admin? Admin { get; set; }
    }
}
