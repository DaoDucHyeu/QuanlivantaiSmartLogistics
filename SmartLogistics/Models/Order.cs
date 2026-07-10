namespace SmartLogistics.Models
{
    public enum OrderStatus
    {
        ChoXuLy,
        DaGan,
        DangGiao,
        HoanThanh,
        Huy
    }

    public class Order
    {
        public int MaDonHang { get; set; }
        public int MaKhachHang { get; set; }
        public string DiemDi { get; set; } = string.Empty;
        public decimal DiemDiLat { get; set; }
        public decimal DiemDiLng { get; set; }
        public string DiemDen { get; set; } = string.Empty;
        public decimal DiemDenLat { get; set; }
        public decimal DiemDenLng { get; set; }
        public decimal KhoiLuong { get; set; }
        public decimal GiaCuoc { get; set; }
        public OrderStatus TrangThai { get; set; } = OrderStatus.ChoXuLy;
        public string? GhiChu { get; set; }
        
        // Mở rộng thêm tuyến đường tĩnh (Hỗ trợ Gom đơn dựa theo tuyến chung)
        public string? TuyenDuong { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime? NgayGiaoHang { get; set; }

        // Navigation properties
        public Customer? KhachHang { get; set; }
        public ICollection<TripDetail> TripDetails { get; set; } = new List<TripDetail>();

        // Mối quan hệ với Điều hành tạo/quản lý đơn hàng này
        public string? DieuHanhId { get; set; }
        public virtual DieuHanh? DieuHanh { get; set; }

        // Multi-Tenant: Thuộc về Admin (Công ty) nào
        public string? AdminId { get; set; }
        public virtual Admin? Admin { get; set; }
    }
}
