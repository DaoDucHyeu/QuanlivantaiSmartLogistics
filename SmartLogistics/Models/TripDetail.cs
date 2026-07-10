namespace SmartLogistics.Models
{
    public enum TripDetailStatus
    {
        ChuaGiao = 0,
        DangGiao = 1,
        DaGiao = 2,
        DangLayHang = 3, // NEW
        Huy = 4
    }

    public class TripDetail
    {
        public int MaChiTietChuyen { get; set; }
        public int MaChuyenDi { get; set; }
        public int MaDonHang { get; set; }
        public int ThuTu { get; set; }
        public TripDetailStatus TrangThai { get; set; } = TripDetailStatus.ChuaGiao;
        
        public DateTime? ThoiGianLayHang { get; set; } // Thêm thuộc tính này
        public DateTime? ThoiGianGiao { get; set; }

        // Navigation properties
        public Trip? ChuyenDi { get; set; }
        public Order? DonHang { get; set; }
    }
}
