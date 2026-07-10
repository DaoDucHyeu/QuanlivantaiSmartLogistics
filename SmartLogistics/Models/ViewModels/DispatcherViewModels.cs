using System.ComponentModel.DataAnnotations;
using SmartLogistics.Models;

namespace SmartLogistics.Models.ViewModels
{
    // ViewModel cho form tạo/sửa đơn hàng
    public class OrderFormViewModel
    {
        public int MaDonHang { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn khách hàng")]
        [Display(Name = "Khách hàng")]
        public int MaKhachHang { get; set; }

        [Required(ErrorMessage = "Điểm đi không được để trống")]
        [Display(Name = "Điểm đi")]
        public string DiemDi { get; set; } = string.Empty;

        [Display(Name = "Vĩ độ điểm đi")]
        public decimal DiemDiLat { get; set; }

        [Display(Name = "Kinh độ điểm đi")]
        public decimal DiemDiLng { get; set; }

        [Required(ErrorMessage = "Điểm đến không được để trống")]
        [Display(Name = "Điểm đến")]
        public string DiemDen { get; set; } = string.Empty;

        [Display(Name = "Vĩ độ điểm đến")]
        public decimal DiemDenLat { get; set; }

        [Display(Name = "Kinh độ điểm đến")]
        public decimal DiemDenLng { get; set; }

        [Required(ErrorMessage = "Khối lượng không được để trống")]
        [Display(Name = "Khối lượng (kg)")]
        [Range(0.1, 100000, ErrorMessage = "Khối lượng phải từ 0.1 đến 100.000 kg")]
        public decimal KhoiLuong { get; set; }

        [Required(ErrorMessage = "Giá cước không được để trống")]
        [Display(Name = "Giá cước (VNĐ)")]
        [Range(1000, 1000000000, ErrorMessage = "Giá cước phải từ 1.000 đến 1.000.000.000 VNĐ")]
        public decimal GiaCuoc { get; set; }

        [Display(Name = "Ngày giao dự kiến")]
        [DataType(DataType.Date)]
        public DateTime? NgayGiaoHang { get; set; }

        [Display(Name = "Ghi chú")]
        [StringLength(500)]
        public string? GhiChu { get; set; }
    }

    // ViewModel cho form tạo/sửa chuyến đi
    public class TripFormViewModel
    {
        public int MaChuyenDi { get; set; }

        public int? MaXe { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn tài xế")]
        [Display(Name = "Tài xế")]
        public string TaiXeId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thời gian bắt đầu không được để trống")]
        [Display(Name = "Thời gian bắt đầu")]
        public DateTime ThoiGianBatDau { get; set; } = DateTime.Now;

        [Display(Name = "Ghi chú")]
        [StringLength(500)]
        public string? GhiChu { get; set; }

        // Danh sách đơn hàng được chọn
        [Display(Name = "Đơn hàng")]
        public List<int> SelectedOrderIds { get; set; } = new();

        // Dữ liệu hiển thị (không binding form)
        public List<Order>? AvailableOrders { get; set; }
        public List<List<Order>>? OrderClusters { get; set; }
    }
}
