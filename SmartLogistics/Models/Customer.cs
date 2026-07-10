namespace SmartLogistics.Models
{
    public class Customer
    {
        public int MaKhachHang { get; set; }
        public string TenKhachHang { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string? DiaChi { get; set; }
        public string? Email { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation properties
        public ICollection<Order> Orders { get; set; } = new List<Order>();

        // Multi-Tenant: Thuộc về Admin (Công ty) nào
        public string? AdminId { get; set; }
        public virtual Admin? Admin { get; set; }
    }
}
