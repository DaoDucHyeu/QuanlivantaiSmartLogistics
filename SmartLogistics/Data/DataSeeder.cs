using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartLogistics.Data;
using SmartLogistics.Models;

namespace SmartLogistics.Seeders
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var UserManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            // Đảm bảo database đã được tạo
            await context.Database.MigrateAsync();

            // Seed Users
            await SeedUsers(UserManager);

            // Seed Customers
            await SeedCustomers(context);

            // Seed Vehicles
            await SeedVehicles(context, UserManager);

            // Seed Orders
            await SeedOrders(context);

            // Seed Trips và TripDetails
            await SeedTripsAndDetails(context);

            await context.SaveChangesAsync();
        }

        private static async Task SeedUsers(UserManager<AppUser> UserManager)
        {
            // Admin
            if (await UserManager.FindByNameAsync("admin") == null)
            {
                var admin = new Admin
                {
                    UserName = "admin",
                    Email = "admin@smartlogistics.vn",
                    FullName = "Nguyễn Văn Quản",
                    UserType = UserType.Admin,
                    EmailConfirmed = true
                };
                await UserManager.CreateAsync(admin, "Admin@123");
            }

            var adminUser = await UserManager.FindByNameAsync("admin");
            var adminId = adminUser?.Id;

            // Điều hành
            var dieuHanhUsers = new[]
            {
                new { UserName = "dieuhanh01", Email = "dieuhanh01@smartlogistics.vn", FullName = "Trần Thị Hương" },
                new { UserName = "dieuhanh02", Email = "dieuhanh02@smartlogistics.vn", FullName = "Lê Văn Minh" }
            };

            foreach (var user in dieuHanhUsers)
            {
                if (await UserManager.FindByNameAsync(user.UserName) == null)
                {
                    var dieuHanh = new DieuHanh
                    {
                        UserName = user.UserName,
                        Email = user.Email,
                        FullName = user.FullName,
                        UserType = UserType.DieuHanh,
                        AdminId = adminId,
                        EmailConfirmed = true
                    };
                    await UserManager.CreateAsync(dieuHanh, "DieuHanh@123");
                }
            }

            // Tài xế
            var taiXeUsers = new[]
            {
                new { UserName = "taixe01", Email = "taixe01@smartlogistics.vn", FullName = "Phạm Văn Hùng", Phone = "0912345001" },
                new { UserName = "taixe02", Email = "taixe02@smartlogistics.vn", FullName = "Hoàng Văn Thắng", Phone = "0912345002" },
                new { UserName = "taixe03", Email = "taixe03@smartlogistics.vn", FullName = "Đặng Văn Tuấn", Phone = "0912345003" },
                new { UserName = "taixe04", Email = "taixe04@smartlogistics.vn", FullName = "Bùi Văn Đức", Phone = "0912345004" },
                new { UserName = "taixe05", Email = "taixe05@smartlogistics.vn", FullName = "Vũ Văn Cường", Phone = "0912345005" },
                new { UserName = "taixe06", Email = "taixe06@smartlogistics.vn", FullName = "Ngô Văn Sơn", Phone = "0912345006" },
                new { UserName = "taixe07", Email = "taixe07@smartlogistics.vn", FullName = "Đinh Văn Long", Phone = "0912345007" },
                new { UserName = "taixe08", Email = "taixe08@smartlogistics.vn", FullName = "Trịnh Văn Hải", Phone = "0912345008" },
                new { UserName = "taixe09", Email = "taixe09@smartlogistics.vn", FullName = "Lý Văn Toán", Phone = "0912345009" }
            };

            // Kéo danh sách ID của Điều hành để gán cho Tài xế
            var dieuHanhList = await UserManager.Users.Where(u => u.UserType == UserType.DieuHanh).ToListAsync();

            foreach (var user in taiXeUsers)
            {
                if (await UserManager.FindByNameAsync(user.UserName) == null)
                {
                    // Tài xế 01-05 thuộc Điều hành 1, 06-09 thuộc Điều hành 2
                    var DieuHanhId = (user.UserName.EndsWith("6") || user.UserName.EndsWith("7") || user.UserName.EndsWith("8") || user.UserName.EndsWith("9")) 
                        ? dieuHanhList.LastOrDefault()?.Id 
                        : dieuHanhList.FirstOrDefault()?.Id;

                    var taiXe = new TaiXe
                    {
                        UserName = user.UserName,
                        Email = user.Email,
                        FullName = user.FullName,
                        PhoneNumber = user.Phone,
                        UserType = UserType.TaiXe,
                        DieuHanhId = DieuHanhId,
                        AdminId = adminId,
                        EmailConfirmed = true
                    };
                    await UserManager.CreateAsync(taiXe, "TaiXe@123");
                }
            }

            // Kế toán
            if (await UserManager.FindByNameAsync("ketoan01") == null)
            {
                var ketoan = new KeToan
                {
                    UserName = "ketoan01",
                    Email = "ketoan01@smartlogistics.vn",
                    FullName = "Nguyễn Thị Lan",
                    UserType = UserType.KeToan,
                    AdminId = adminId,
                    EmailConfirmed = true
                };
                await UserManager.CreateAsync(ketoan, "KeToan@123");
            }
        }

        private static async Task SeedCustomers(ApplicationDbContext context)
        {
            if (await context.Customers.AnyAsync()) return;

            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.UserName == "admin");
            var adminId = adminUser?.Id;

            var customers = new[]
            {
                new Customer { TenKhachHang = "Công ty TNHH Thương mại Hà Nội", SoDienThoai = "0241234567", Email = "contact@tmhn.vn", DiaChi = "Số 15 Trần Đại Nghĩa, Hai Bà Trưng, Hà Nội", NgayTao = DateTime.Now.AddMonths(-6), AdminId = adminId },
                new Customer { TenKhachHang = "Công ty CP Điện tử Việt Nam", SoDienThoai = "0243456789", Email = "info@electronics.vn", DiaChi = "Tầng 5, Tòa nhà HITC, Xuân Thủy, Cầu Giấy, Hà Nội", NgayTao = DateTime.Now.AddMonths(-5), AdminId = adminId },
                new Customer { TenKhachHang = "Siêu thị Điện máy Xanh HN", SoDienThoai = "0245678901", Email = "hanoi@dmx.vn", DiaChi = "234 Nguyễn Trãi, Thanh Xuân, Hà Nội", NgayTao = DateTime.Now.AddMonths(-4), AdminId = adminId },
                new Customer { TenKhachHang = "Công ty TNHH Dược phẩm Hải Phòng", SoDienThoai = "0225123456", Email = "sales@pharma-hp.vn", DiaChi = "56 Lạch Tray, Ngô Quyền, Hải Phòng", NgayTao = DateTime.Now.AddMonths(-4), AdminId = adminId },
                new Customer { TenKhachHang = "Nhà máy Sản xuất Nhựa Bình Dương", SoDienThoai = "0274234567", Email = "contact@plastic-bd.vn", DiaChi = "KCN Việt Nam Singapore, Thuận An, Bình Dương", NgayTao = DateTime.Now.AddMonths(-3), AdminId = adminId },
                new Customer { TenKhachHang = "Công ty CP Vật liệu Xây dựng Miền Bắc", SoDienThoai = "0246789012", Email = "info@buildmat.vn", DiaChi = "Km 12 Quốc lộ 5, Gia Lâm, Hà Nội", NgayTao = DateTime.Now.AddMonths(-3), AdminId = adminId },
                new Customer { TenKhachHang = "Tập đoàn Thực phẩm Việt", SoDienThoai = "0247890123", Email = "order@foodviet.vn", DiaChi = "Lô E2, KCN Thăng Long, Đông Anh, Hà Nội", NgayTao = DateTime.Now.AddMonths(-2), AdminId = adminId },
                new Customer { TenKhachHang = "Công ty TNHH May mặc Hòa Bình", SoDienThoai = "0218901234", Email = "export@garment-hb.vn", DiaChi = "Số 78 Đường Cù Chính Lan, TP Hòa Bình", NgayTao = DateTime.Now.AddMonths(-2), AdminId = adminId },
                new Customer { TenKhachHang = "Siêu thị Co.opMart Hà Đông", SoDienThoai = "0249012345", Email = "hadong@coopmart.vn", DiaChi = "Tầng 1-2 AEON Mall Hà Đông, Dương Nội, Hà Đông, Hà Nội", NgayTao = DateTime.Now.AddMonths(-2), AdminId = adminId },
                new Customer { TenKhachHang = "Công ty CP Nội thất Hòa Phát", SoDienThoai = "0240123456", Email = "sales@hoaphathome.vn", DiaChi = "Số 8 Thái Hà, Đống Đa, Hà Nội", NgayTao = DateTime.Now.AddMonths(-1), AdminId = adminId },
                new Customer { TenKhachHang = "Nhà máy Sản xuất Giấy Nam Định", SoDienThoai = "0228234567", Email = "contact@paper-nd.vn", DiaChi = "KCN Nam Định, TP Nam Định", NgayTao = DateTime.Now.AddMonths(-1), AdminId = adminId },
                new Customer { TenKhachHang = "Công ty TNHH Hóa chất Bắc Ninh", SoDienThoai = "0222345678", Email = "info@chemical-bn.vn", DiaChi = "KCN Quế Võ, Bắc Ninh", NgayTao = DateTime.Now.AddDays(-20), AdminId = adminId },
                new Customer { TenKhachHang = "Kho bãi Logistics Phú Thọ", SoDienThoai = "0210456789", Email = "warehouse@pt-logistics.vn", DiaChi = "Km 85 Quốc lộ 2, Việt Trì, Phú Thọ", NgayTao = DateTime.Now.AddDays(-15), AdminId = adminId },
                new Customer { TenKhachHang = "Công ty CP Thiết bị Y tế Việt Đức", SoDienThoai = "0241567890", Email = "sales@vietducmedical.vn", DiaChi = "Tầng 12, Tòa nhà Detech, Tôn Thất Thuyết, Cầu Giấy, Hà Nội", NgayTao = DateTime.Now.AddDays(-10), AdminId = adminId },
                new Customer { TenKhachHang = "Siêu thị Thế giới Di động Hà Nội", SoDienThoai = "0242678901", Email = "hanoi@thegioididong.com", DiaChi = "128 Trần Duy Hưng, Cầu Giấy, Hà Nội", NgayTao = DateTime.Now.AddDays(-5), AdminId = adminId }
            };

            await context.Customers.AddRangeAsync(customers);
            await context.SaveChangesAsync();
        }

        private static async Task SeedVehicles(ApplicationDbContext context, UserManager<AppUser> UserManager)
        {
            if (await context.Vehicles.AnyAsync()) return;

            var taiXeList = await UserManager.Users.Where(u => u.UserType == UserType.TaiXe).ToListAsync();

            var dieuHanhList = await UserManager.Users.Where(u => u.UserType == UserType.DieuHanh).ToListAsync();
            var dh1 = dieuHanhList.FirstOrDefault();
            var dh2 = dieuHanhList.LastOrDefault();
            var dh1Id = dh1?.Id;
            var dh2Id = dh2?.Id;
            var adminId = dh1?.AdminId; // Lấy AdminId từ Điều Hành

            var vehicles = new[]
            {
                new Vehicle { BienSo = "29C-12345", LoaiXe = "Xe tải 1.5 tấn", TrongTai = 1.5m, TrangThai = VehicleStatus.SanSang, TaiXeId = taiXeList[0].Id, DieuHanhId = dh2Id, AdminId = adminId, NgayDangKy = DateTime.Now.AddYears(-2) },
                new Vehicle { BienSo = "30H-23456", LoaiXe = "Xe tải 2.5 tấn", TrongTai = 2.5m, TrangThai = VehicleStatus.SanSang, TaiXeId = taiXeList[1].Id, DieuHanhId = dh1Id, AdminId = adminId, NgayDangKy = DateTime.Now.AddYears(-2) },
                new Vehicle { BienSo = "29C-34567", LoaiXe = "Xe tải 3.5 tấn", TrongTai = 3.5m, TrangThai = VehicleStatus.DangDi, TaiXeId = taiXeList[2].Id, DieuHanhId = dh2Id, AdminId = adminId, NgayDangKy = DateTime.Now.AddYears(-1) },
                new Vehicle { BienSo = "30G-45678", LoaiXe = "Xe tải 5 tấn", TrongTai = 5.0m, TrangThai = VehicleStatus.SanSang, TaiXeId = taiXeList[3].Id, DieuHanhId = dh1Id, AdminId = adminId, NgayDangKy = DateTime.Now.AddYears(-1) },
                new Vehicle { BienSo = "29C-56789", LoaiXe = "Xe tải 7 tấn", TrongTai = 7.0m, TrangThai = VehicleStatus.DangDi, TaiXeId = taiXeList[4].Id, DieuHanhId = dh2Id, AdminId = adminId, NgayDangKy = DateTime.Now.AddMonths(-10) },
                new Vehicle { BienSo = "30K-67890", LoaiXe = "Xe tải 10 tấn", TrongTai = 10.0m, TrangThai = VehicleStatus.SanSang, TaiXeId = taiXeList[5].Id, DieuHanhId = dh1Id, AdminId = adminId, NgayDangKy = DateTime.Now.AddMonths(-8) },
                new Vehicle { BienSo = "29C-78901", LoaiXe = "Xe tải 1.5 tấn", TrongTai = 1.5m, TrangThai = VehicleStatus.SanSang, TaiXeId = taiXeList[6].Id, DieuHanhId = dh2Id, AdminId = adminId, NgayDangKy = DateTime.Now.AddMonths(-6) },
                new Vehicle { BienSo = "30H-89012", LoaiXe = "Xe tải 3.5 tấn", TrongTai = 3.5m, TrangThai = VehicleStatus.BaoTri, TaiXeId = taiXeList[7].Id, DieuHanhId = dh1Id, AdminId = adminId, NgayDangKy = DateTime.Now.AddMonths(-4) },
                new Vehicle { BienSo = "29C-90123", LoaiXe = "Xe tải 2.5 tấn", TrongTai = 2.5m, TrangThai = VehicleStatus.SanSang, TaiXeId = null, DieuHanhId = dh1Id, AdminId = adminId, NgayDangKy = DateTime.Now.AddMonths(-3) },
                new Vehicle { BienSo = "30G-01234", LoaiXe = "Xe tải 5 tấn", TrongTai = 5.0m, TrangThai = VehicleStatus.SanSang, TaiXeId = null, DieuHanhId = dh2Id, AdminId = adminId, NgayDangKy = DateTime.Now.AddMonths(-2) }
            };

            await context.Vehicles.AddRangeAsync(vehicles);
            await context.SaveChangesAsync();
        }

        private static async Task SeedOrders(ApplicationDbContext context)
        {
            if (await context.Orders.AnyAsync()) return;

            var customers = await context.Customers.ToListAsync();
            var random = new Random();

            // Tọa độ các địa điểm thực tế ở Hà Nội và vùng lân cận
            var locations = new[]
            {
                new { Name = "Hai Bà Trưng, Hà Nội", Lat = 21.0122m, Lng = 105.8542m },
                new { Name = "Cầu Giấy, Hà Nội", Lat = 21.0285m, Lng = 105.7952m },
                new { Name = "Thanh Xuân, Hà Nội", Lat = 20.9952m, Lng = 105.8052m },
                new { Name = "Đống Đa, Hà Nội", Lat = 21.0170m, Lng = 105.8270m },
                new { Name = "Hoàn Kiếm, Hà Nội", Lat = 21.0285m, Lng = 105.8542m },
                new { Name = "Long Biên, Hà Nội", Lat = 21.0365m, Lng = 105.8938m },
                new { Name = "Gia Lâm, Hà Nội", Lat = 21.0208m, Lng = 105.9633m },
                new { Name = "Hà Đông, Hà Nội", Lat = 20.9719m, Lng = 105.7692m },
                new { Name = "Đông Anh, Hà Nội", Lat = 21.1372m, Lng = 105.8463m },
                new { Name = "Hải Phòng", Lat = 20.8449m, Lng = 106.6881m },
                new { Name = "Bắc Ninh", Lat = 21.1861m, Lng = 106.0763m },
                new { Name = "Hưng Yên", Lat = 20.6464m, Lng = 106.0511m },
                new { Name = "Hải Dương", Lat = 20.9373m, Lng = 106.3145m },
                new { Name = "Nam Định", Lat = 20.4388m, Lng = 106.1621m },
                new { Name = "Thái Bình", Lat = 20.4463m, Lng = 106.3365m },
                new { Name = "Ninh Bình", Lat = 20.2506m, Lng = 105.9745m },
                new { Name = "Phú Thọ", Lat = 21.4208m, Lng = 105.2045m },
                new { Name = "Vĩnh Phúc", Lat = 21.3609m, Lng = 105.5474m },
                new { Name = "Bắc Giang", Lat = 21.2819m, Lng = 106.1975m },
                new { Name = "Thái Nguyên", Lat = 21.5671m, Lng = 105.8252m }
            };

            var orders = new List<Order>();

            for (int i = 0; i < 30; i++)
            {
                var customer = customers[random.Next(customers.Count)];
                var diemDi = locations[random.Next(locations.Length)];
                var diemDen = locations[random.Next(locations.Length)];
                
                while (diemDi.Name == diemDen.Name)
                {
                    diemDen = locations[random.Next(locations.Length)];
                }

                var khoiLuong = random.Next(100, 5000);
                var khoangCach = Math.Abs(diemDi.Lat - diemDen.Lat) + Math.Abs(diemDi.Lng - diemDen.Lng);
                var giaCuoc = (decimal)(khoiLuong * 5 + (double)khoangCach * 100000);

                var ngayTao = DateTime.Now.AddDays(-random.Next(1, 30));
                var ngayGiao = ngayTao.AddDays(random.Next(1, 5));

                OrderStatus trangThai;
                if (i < 5)
                    trangThai = OrderStatus.ChoXuLy;
                else if (i < 10)
                    trangThai = OrderStatus.DaGan;
                else if (i < 15)
                    trangThai = OrderStatus.DangGiao;
                else if (i < 28)
                    trangThai = OrderStatus.HoanThanh;
                else
                    trangThai = OrderStatus.Huy;

                // 15 đơn đầu cho Điều hành 1, 15 đơn sau cho Điều hành 2
                var DieuHanhId = i % 2 == 0 ? "2" : "1"; // Sẽ update ID thực tế ngay sau foreach
                orders.Add(new Order
                {
                    MaKhachHang = customer.MaKhachHang,
                    DiemDi = diemDi.Name,
                    DiemDiLat = diemDi.Lat,
                    DiemDiLng = diemDi.Lng,
                    DiemDen = diemDen.Name,
                    DiemDenLat = diemDen.Lat,
                    DiemDenLng = diemDen.Lng,
                    KhoiLuong = khoiLuong,
                    GiaCuoc = giaCuoc,
                    TrangThai = trangThai,
                    GhiChu = i % 3 == 0 ? "Hàng dễ vỡ, cần cẩn thận" : i % 5 == 0 ? "Giao hàng trong giờ hành chính" : null,
                    TuyenDuong = $"{diemDi.Name} - {diemDen.Name}",
                    NgayTao = ngayTao,
                    NgayGiaoHang = ngayGiao,
                    DieuHanhId = DieuHanhId // Tạm gán
                });
            }

            await context.Orders.AddRangeAsync(orders);
            await context.SaveChangesAsync();

            var dieuHanhList = await context.Users.Where(u => u.UserType == UserType.DieuHanh).ToListAsync();
            var dh1Id = dieuHanhList.FirstOrDefault()?.Id;
            var dh2Id = dieuHanhList.LastOrDefault()?.Id;

            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.UserName == "admin");
            var adminId = adminUser?.Id;

            foreach (var order in orders)
            {
                order.DieuHanhId = order.DieuHanhId == "1" ? dh1Id : dh2Id;
                order.AdminId = adminId; // Add AdminId
            }
            await context.SaveChangesAsync();
        }

        private static async Task SeedTripsAndDetails(ApplicationDbContext context)
        {
            if (await context.Trips.AnyAsync()) return;

            var vehicles = await context.Vehicles.Include(v => v.TaiXe).Where(v => v.TaiXeId != null).ToListAsync();
            var orders = await context.Orders.Where(o => o.TrangThai == OrderStatus.DaGan || o.TrangThai == OrderStatus.DangGiao || o.TrangThai == OrderStatus.HoanThanh).ToListAsync();

            var trips = new List<Trip>();
            var tripDetails = new List<TripDetail>();
            var random = new Random();

            // Tạo 10 chuyến đi
            for (int i = 0; i < 10 && i < vehicles.Count; i++)
            {
                var vehicle = vehicles[i];
                var ngayBatDau = DateTime.Now.AddDays(-random.Next(1, 20));
                var trangThai = i < 2 ? TripStatus.DangThucHien : i < 8 ? TripStatus.HoanThanh : TripStatus.ChuaBatDau;
                
                var trip = new Trip
                {
                    MaXe = vehicle.MaXe,
                    TaiXeId = vehicle.TaiXeId!,
                    ThoiGianBatDau = ngayBatDau,
                    ThoiGianKetThuc = trangThai == TripStatus.HoanThanh ? ngayBatDau.AddHours(random.Next(4, 12)) : null,
                    TrangThai = trangThai,
                    TongKhoangCach = random.Next(50, 300),
                    GhiChu = i % 2 == 0 ? "Chuyến đi thuận lợi" : null,
                    DieuHanhId = vehicle.DieuHanhId,
                    AdminId = vehicle.AdminId
                };

                trips.Add(trip);
            }

            await context.Trips.AddRangeAsync(trips);
            await context.SaveChangesAsync();

            // Gán đơn hàng vào chuyến đi
            int orderIndex = 0;
            foreach (var trip in trips)
            {
                int soLuongDon = random.Next(1, 4);
                for (int j = 0; j < soLuongDon && orderIndex < orders.Count; j++)
                {
                    var order = orders[orderIndex++];
                    
                    tripDetails.Add(new TripDetail
                    {
                        MaChuyenDi = trip.MaChuyenDi,
                        MaDonHang = order.MaDonHang,
                        ThuTu = j + 1,
                        TrangThai = trip.TrangThai == TripStatus.HoanThanh ? TripDetailStatus.DaGiao : 
                                   trip.TrangThai == TripStatus.DangThucHien ? TripDetailStatus.DangGiao : 
                                   TripDetailStatus.ChuaGiao,
                        ThoiGianGiao = trip.TrangThai == TripStatus.HoanThanh ? trip.ThoiGianKetThuc?.AddHours(-random.Next(1, 3)) : null
                    });
                }
            }

            await context.TripDetails.AddRangeAsync(tripDetails);
            await context.SaveChangesAsync();
        }
    }
}
