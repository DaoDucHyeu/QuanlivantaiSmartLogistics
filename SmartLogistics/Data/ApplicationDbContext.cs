using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartLogistics.Models;

namespace SmartLogistics.Data
{
    public class ApplicationDbContext : IdentityUserContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet cho người dùng TPT (Table-Per-Type)
        public DbSet<Admin> Admins { get; set; }
        public DbSet<DieuHanh> DieuHanhs { get; set; }
        public DbSet<TaiXe> TaiXes { get; set; }
        public DbSet<KeToan> KeToans { get; set; }

        // DbSet cho các entity
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<TripDetail> TripDetails { get; set; }
        public DbSet<Incident> Incidents { get; set; }
        public DbSet<TripLocation> TripLocations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Đổi tên các bảng mặc định còn lại của Identity cho gọn gàng và dễ nhìn hơn
            builder.Entity<AppUser>().ToTable("Users");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

            // Cấu hình TPT Mapping cho IdentityUser
            builder.Entity<Admin>().ToTable("Admins");
            builder.Entity<KeToan>().ToTable("KeToans");
            builder.Entity<DieuHanh>().ToTable("DieuHanhs");
            builder.Entity<TaiXe>().ToTable("TaiXes");

            // Multi-Tenant: Cấu hình AppUser -> Admin
            builder.Entity<AppUser>(entity =>
            {
                entity.HasOne(e => e.Admin)
                    .WithMany()
                    .HasForeignKey(e => e.AdminId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => e.AdminId);
            });

            // Cấu hình TaiXe -> DieuHanh (Self-referencing đã chuyển thành TaiXe->DieuHanh)
            builder.Entity<TaiXe>(entity =>
            {
                entity.HasOne(e => e.DieuHanh)
                    .WithMany(m => m.ManagedDrivers)
                    .HasForeignKey(e => e.DieuHanhId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Cấu hình Customer
            builder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.MaKhachHang);
                entity.Property(e => e.TenKhachHang).IsRequired().HasMaxLength(200);
                entity.Property(e => e.SoDienThoai).IsRequired().HasMaxLength(20);
                entity.Property(e => e.DiaChi).HasMaxLength(500);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.HasIndex(e => e.SoDienThoai);

                entity.HasOne(e => e.Admin)
                    .WithMany()
                    .HasForeignKey(e => e.AdminId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => e.AdminId);
            });

            // Cấu hình Vehicle
            builder.Entity<Vehicle>(entity =>
            {
                entity.HasKey(e => e.MaXe);
                entity.Property(e => e.BienSo).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.BienSo).IsUnique();
                entity.Property(e => e.LoaiXe).HasMaxLength(100);
                entity.Property(e => e.TrongTai).HasColumnType("decimal(10,2)");
                entity.Property(e => e.TrangThai).HasConversion<string>();
                
                entity.HasOne(e => e.TaiXe)
                    .WithMany()
                    .HasForeignKey(e => e.TaiXeId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.DieuHanh)
                    .WithMany()
                    .HasForeignKey(e => e.DieuHanhId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Admin)
                    .WithMany()
                    .HasForeignKey(e => e.AdminId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.TaiXeId);
                entity.HasIndex(e => e.DieuHanhId);
                entity.HasIndex(e => e.AdminId);
            });

            // Cấu hình Order
            builder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.MaDonHang);
                entity.Property(e => e.DiemDi).IsRequired().HasMaxLength(500);
                entity.Property(e => e.DiemDiLat).HasColumnType("decimal(10,7)");
                entity.Property(e => e.DiemDiLng).HasColumnType("decimal(10,7)");
                entity.Property(e => e.DiemDen).IsRequired().HasMaxLength(500);
                entity.Property(e => e.DiemDenLat).HasColumnType("decimal(10,7)");
                entity.Property(e => e.DiemDenLng).HasColumnType("decimal(10,7)");
                entity.Property(e => e.KhoiLuong).HasColumnType("decimal(10,2)");
                entity.Property(e => e.GiaCuoc).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TrangThai).HasConversion<string>();
                entity.Property(e => e.GhiChu).HasMaxLength(1000);
                entity.Property(e => e.TuyenDuong).HasMaxLength(200);

                entity.HasOne(e => e.KhachHang)
                    .WithMany(c => c.Orders)
                    .HasForeignKey(e => e.MaKhachHang)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.DieuHanh)
                    .WithMany()
                    .HasForeignKey(e => e.DieuHanhId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Admin)
                    .WithMany()
                    .HasForeignKey(e => e.AdminId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.MaKhachHang);
                entity.HasIndex(e => e.DieuHanhId);
                entity.HasIndex(e => e.AdminId);
                entity.HasIndex(e => e.TrangThai);
                entity.HasIndex(e => e.NgayTao);
            });

            // Cấu hình Trip
            builder.Entity<Trip>(entity =>
            {
                entity.HasKey(e => e.MaChuyenDi);
                entity.Property(e => e.TrangThai).HasConversion<string>();
                entity.Property(e => e.TongKhoangCach).HasColumnType("decimal(10,2)");
                entity.Property(e => e.GhiChu).HasMaxLength(1000);

                entity.HasOne(e => e.Xe)
                    .WithMany(v => v.Trips)
                    .HasForeignKey(e => e.MaXe)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.TaiXe)
                    .WithMany()
                    .HasForeignKey(e => e.TaiXeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.DieuHanh)
                    .WithMany()
                    .HasForeignKey(e => e.DieuHanhId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Admin)
                    .WithMany()
                    .HasForeignKey(e => e.AdminId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.MaXe);
                entity.HasIndex(e => e.TaiXeId);
                entity.HasIndex(e => e.DieuHanhId);
                entity.HasIndex(e => e.AdminId);
                entity.HasIndex(e => e.TrangThai);
            });

            // Cấu hình TripDetail
            builder.Entity<TripDetail>(entity =>
            {
                entity.HasKey(e => e.MaChiTietChuyen);
                entity.Property(e => e.TrangThai).HasConversion<string>();

                entity.HasOne(e => e.ChuyenDi)
                    .WithMany(t => t.TripDetails)
                    .HasForeignKey(e => e.MaChuyenDi)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.DonHang)
                    .WithMany(o => o.TripDetails)
                    .HasForeignKey(e => e.MaDonHang)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.MaChuyenDi);
                entity.HasIndex(e => e.MaDonHang);
                entity.HasIndex(e => new { e.MaChuyenDi, e.MaDonHang }).IsUnique();
            });

            // Cấu hình Incident
            builder.Entity<Incident>(entity =>
            {
                entity.HasKey(e => e.MaSuCo);
                entity.Property(e => e.LoaiSuCo).HasConversion<string>();
                entity.Property(e => e.TrangThai).HasConversion<string>();
                entity.Property(e => e.MoTa).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.HinhAnh).HasMaxLength(500);
                entity.Property(e => e.ViTriLat).HasColumnType("decimal(10,7)");
                entity.Property(e => e.ViTriLng).HasColumnType("decimal(10,7)");

                entity.HasOne(e => e.ChuyenDi)
                    .WithMany(t => t.Incidents)
                    .HasForeignKey(e => e.MaChuyenDi)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.MaChuyenDi);
                entity.HasIndex(e => e.ThoiGian);
            });

            // Cấu hình TripLocation
            builder.Entity<TripLocation>(entity =>
            {
                entity.HasKey(e => e.MaToaDo);
                entity.Property(e => e.Latitude).HasColumnType("decimal(10,7)");
                entity.Property(e => e.Longitude).HasColumnType("decimal(10,7)");
                entity.Property(e => e.TocDo).HasColumnType("decimal(6,2)");

                entity.HasOne(e => e.ChuyenDi)
                    .WithMany(t => t.TripLocations)
                    .HasForeignKey(e => e.MaChuyenDi)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.MaChuyenDi);
                entity.HasIndex(e => e.ThoiGian);
            });
        }
    }
}
