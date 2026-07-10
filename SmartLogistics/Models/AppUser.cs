using Microsoft.AspNetCore.Identity;

namespace SmartLogistics.Models
{
    public enum UserType
    {
        Admin,
        DieuHanh,
        TaiXe,
        KeToan
    }

    public class AppUser : IdentityUser
    {
        public string? FullName { get; set; }
        public UserType UserType { get; set; }
        
        // Multi-Tenant: Khóa ngoại trỏ đến Admin (Công ty vận tải) mà user này thuộc về
        public string? AdminId { get; set; }
        public virtual Admin? Admin { get; set; }
    }

    public class Admin : AppUser
    {
    }

    public class KeToan : AppUser
    {
    }

    public class DieuHanh : AppUser
    {
        // Danh sách Tài xế mà Điều hành này đang quản lý
        public virtual ICollection<TaiXe> ManagedDrivers { get; set; } = new List<TaiXe>();
    }

    public class TaiXe : AppUser
    {
        // ID của Điều hành quản lý
        public string? DieuHanhId { get; set; }
        public virtual DieuHanh? DieuHanh { get; set; }
    }
}
