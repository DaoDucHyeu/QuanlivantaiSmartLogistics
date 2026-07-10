namespace SmartLogistics.Models
{
    public enum VehicleStatus
    {
        SanSang,
        DangDi,
        BaoTri
    }

    public class Vehicle
    {
        public int MaXe { get; set; }
        public string BienSo { get; set; } = string.Empty;
        public string? LoaiXe { get; set; }
        public decimal TrongTai { get; set; }
        public VehicleStatus TrangThai { get; set; } = VehicleStatus.SanSang;
        public string? TaiXeId { get; set; }
        public DateTime NgayDangKy { get; set; } = DateTime.Now;

        // Navigation properties
        public TaiXe? TaiXe { get; set; }
        public ICollection<Trip> Trips { get; set; } = new List<Trip>();

        // Mối quan hệ với Điều hành quản lý xe này
        public string? DieuHanhId { get; set; }
        public virtual DieuHanh? DieuHanh { get; set; }

        // Multi-Tenant: Thuộc về Admin (Công ty) nào
        public string? AdminId { get; set; }
        public virtual Admin? Admin { get; set; }
    }
}
