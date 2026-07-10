using System.ComponentModel.DataAnnotations;
using SmartLogistics.Models;

namespace SmartLogistics.Models.ViewModels
{
    // ViewModel cho trang Dashboard
    public class DashboardViewModel
    {
        public int TotalStaff { get; set; }
        public int ActiveVehicles { get; set; }
        public int TotalVehicles { get; set; }
        public int PendingOrders { get; set; }
        public int ActiveTrips { get; set; }
        public int CompletedOrders { get; set; }
        public int DeliveringOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int AssignedOrders { get; set; }
    }

    // ViewModel cho Thêm/Sửa nhân sự
    public class StaffCreateViewModel
    {
        [Required(ErrorMessage = "Họ và tên không được để trống")]
        [Display(Name = "Họ và tên")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [Display(Name = "Tên đăng nhập")]
        [StringLength(50)]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        [Display(Name = "Vai trò")]
        public UserType UserType { get; set; }

        [Display(Name = "Điều hành quản lý (Dành cho Tài xế)")]
        public string? DieuHanhId { get; set; }
    }

    // ViewModel cho Chỉnh sửa nhân sự
    public class StaffEditViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ và tên không được để trống")]
        [Display(Name = "Họ và tên")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [Display(Name = "Tên đăng nhập")]
        [StringLength(50)]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? PhoneNumber { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới (để trống nếu không đổi)")]
        public string? NewPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        [Display(Name = "Vai trò")]
        public UserType UserType { get; set; }

        [Display(Name = "Điều hành quản lý (Dành cho Tài xế)")]
        public string? DieuHanhId { get; set; }
    }

    // ViewModel cho Thêm/Sửa phương tiện
    public class VehicleFormViewModel
    {
        public int MaXe { get; set; }

        [Required(ErrorMessage = "Biển số xe không được để trống")]
        [Display(Name = "Biển số xe")]
        [StringLength(20)]
        public string BienSo { get; set; } = string.Empty;

        [Display(Name = "Loại xe")]
        [StringLength(100)]
        public string? LoaiXe { get; set; }

        [Required(ErrorMessage = "Trọng tải không được để trống")]
        [Display(Name = "Trọng tải (tấn)")]
        [Range(0.1, 100, ErrorMessage = "Trọng tải phải từ 0.1 đến 100 tấn")]
        public decimal TrongTai { get; set; }

        [Display(Name = "Tài xế phụ trách")]
        public string? TaiXeId { get; set; }

        [Display(Name = "Trạng thái")]
        public VehicleStatus TrangThai { get; set; } = VehicleStatus.SanSang;
    }
}
