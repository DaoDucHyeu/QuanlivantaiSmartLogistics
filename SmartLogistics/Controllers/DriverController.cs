using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartLogistics.Data;
using SmartLogistics.Models;
using SmartLogistics.Models.ViewModels;
using SmartLogistics.Helpers;

namespace SmartLogistics.Controllers
{
    [AuthorizeUserType(UserType.TaiXe)]
    public class DriverController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _UserManager;

        public DriverController(ApplicationDbContext context, UserManager<AppUser> UserManager)
        {
            _context = context;
            _UserManager = UserManager;
        }

        // GET: /Driver
        public async Task<IActionResult> Index()
        {
            var user = await _UserManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var today = DateTime.Today;

            // Xe đang gán cho tài xế
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.TaiXeId == user.Id);

            // Chuyến đi hôm nay
            var todayTrips = await _context.Trips
                .Where(t => t.TaiXeId == user.Id && t.ThoiGianBatDau.Date == today)
                .CountAsync();

            // Chuyến đang thực hiện
            var currentTrip = await _context.Trips
                .Include(t => t.Xe)
                .Include(t => t.TripDetails)
                .Where(t => t.TaiXeId == user.Id && t.TrangThai == TripStatus.DangThucHien)
                .FirstOrDefaultAsync();

            // Chuyến sắp tới
            var upcomingTrips = await _context.Trips
                .Include(t => t.TripDetails)
                .Where(t => t.TaiXeId == user.Id && t.TrangThai == TripStatus.ChuaBatDau)
                .OrderBy(t => t.ThoiGianBatDau)
                .Take(3)
                .ToListAsync();

            // Thống kê
            var completedTrips = await _context.Trips
                .CountAsync(t => t.TaiXeId == user.Id && t.TrangThai == TripStatus.HoanThanh);

            var incidentCount = await _context.Incidents
                .CountAsync(i => i.ChuyenDi!.TaiXeId == user.Id && i.TrangThai == IncidentStatus.DangXuLy);

            var model = new DriverDashboardViewModel
            {
                DriverName = user.FullName ?? "Tài xế",
                VehiclePlate = vehicle?.BienSo,
                TripsToday = todayTrips,
                InProgressTrips = currentTrip != null ? 1 : 0,
                CompletedTrips = completedTrips,
                IncidentCount = incidentCount,
                CurrentTrip = currentTrip,
                UpcomingTrips = upcomingTrips
            };

            return View(model);
        }

        // GET: /Driver/Trips
        public async Task<IActionResult> Trips(string? tab)
        {
            var user = await _UserManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var query = _context.Trips
                .Include(t => t.Xe)
                .Include(t => t.TripDetails)
                .Where(t => t.TaiXeId == user.Id);

            if (tab == "active")
                query = query.Where(t => t.TrangThai == TripStatus.DangThucHien);
            else if (tab == "done")
                query = query.Where(t => t.TrangThai == TripStatus.HoanThanh);

            var trips = await query.OrderByDescending(t => t.ThoiGianBatDau).ToListAsync();

            ViewBag.Tab = tab ?? "all";
            ViewData["ShowBack"] = true;
            ViewData["BackUrl"] = Url.Action("Index");
            ViewData["TopTitle"] = "Chuyến đi của tôi";

            return View(trips);
        }

        // GET: /Driver/TripDetail/5
        public async Task<IActionResult> TripDetail(int id)
        {
            var user = await _UserManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var trip = await _context.Trips
                .Include(t => t.Xe)
                .Include(t => t.TripDetails)
                    .ThenInclude(td => td.DonHang)
                        .ThenInclude(o => o!.KhachHang)
                .FirstOrDefaultAsync(t => t.MaChuyenDi == id && t.TaiXeId == user.Id);

            if (trip == null) return NotFound();

            ViewData["ShowBack"] = true;
            ViewData["BackUrl"] = Url.Action("Trips");
            ViewData["TopTitle"] = $"Chi tiết chuyến đi";

            return View(trip);
        }

        // POST: /Driver/ConfirmDelivery
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDelivery(int tripDetailId)
        {
            var user = await _UserManager.GetUserAsync(User);
            var td = await _context.TripDetails
                .Include(x => x.ChuyenDi)
                .Include(x => x.DonHang)
                .FirstOrDefaultAsync(x => x.MaChiTietChuyen == tripDetailId);

            if (td == null || td.ChuyenDi?.TaiXeId != user?.Id)
                return NotFound();

            td.TrangThai = TripDetailStatus.DaGiao;
            td.ThoiGianGiao = DateTime.Now;

            if (td.DonHang != null)
                td.DonHang.TrangThai = OrderStatus.HoanThanh;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã xác nhận giao hàng #DH{td.MaDonHang:D3}!";
            return RedirectToAction(nameof(TripDetail), new { id = td.MaChuyenDi });
        }
        // POST: /Driver/ConfirmPickup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPickup(int tripDetailId)
        {
            var user = await _UserManager.GetUserAsync(User);
            var td = await _context.TripDetails
                .Include(x => x.ChuyenDi)
                .FirstOrDefaultAsync(x => x.MaChiTietChuyen == tripDetailId);

            if (td == null || td.ChuyenDi?.TaiXeId != user?.Id)
                return NotFound();

            td.TrangThai = TripDetailStatus.DangGiao;
            td.ThoiGianLayHang = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã xác nhận LẤY HÀNG đơn #DH{td.MaDonHang:D3}!";
            return RedirectToAction(nameof(TripDetail), new { id = td.MaChuyenDi });
        }

        // POST: /Driver/StartTrip/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartTrip(int id)
        {
            var user = await _UserManager.GetUserAsync(User);
            var trip = await _context.Trips
                .Include(t => t.TripDetails)
                .FirstOrDefaultAsync(t => t.MaChuyenDi == id && t.TaiXeId == user!.Id);

            if (trip == null) return NotFound();

            if (trip.TrangThai != TripStatus.ChuaBatDau)
            {
                TempData["Error"] = "Chuyến đi không ở trạng thái chờ bắt đầu!";
                return RedirectToAction(nameof(TripDetail), new { id });
            }

            trip.TrangThai = TripStatus.DangThucHien;
            trip.ThoiGianBatDau = DateTime.Now;

            foreach (var td in trip.TripDetails)
            {
                var order = await _context.Orders.FindAsync(td.MaDonHang);
                if (order != null) order.TrangThai = OrderStatus.DangGiao;
                td.TrangThai = TripDetailStatus.DangLayHang; // Chuyển sang Đang đi lấy hàng (Điểm 1)
            }

            var vehicle = await _context.Vehicles.FindAsync(trip.MaXe);
            if (vehicle != null) vehicle.TrangThai = VehicleStatus.DangDi;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã bắt đầu chuyến đi!";
            return RedirectToAction(nameof(TripDetail), new { id });
        }

        // POST: /Driver/CompleteTrip/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteTrip(int id)
        {
            var user = await _UserManager.GetUserAsync(User);
            var trip = await _context.Trips
                .Include(t => t.TripDetails)
                .FirstOrDefaultAsync(t => t.MaChuyenDi == id && t.TaiXeId == user!.Id);

            if (trip == null) return NotFound();

            trip.TrangThai = TripStatus.HoanThanh;
            trip.ThoiGianKetThuc = DateTime.Now;

            foreach (var td in trip.TripDetails)
            {
                if (td.TrangThai != TripDetailStatus.DaGiao)
                {
                    td.TrangThai = TripDetailStatus.DaGiao;
                    td.ThoiGianGiao = DateTime.Now;
                }
                var order = await _context.Orders.FindAsync(td.MaDonHang);
                if (order != null) order.TrangThai = OrderStatus.HoanThanh;
            }

            var vehicle = await _context.Vehicles.FindAsync(trip.MaXe);
            if (vehicle != null) vehicle.TrangThai = VehicleStatus.SanSang;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã hoàn thành chuyến đi!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Driver/Profile
        public async Task<IActionResult> Profile()
        {
            var user = await _UserManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.TaiXeId == user.Id);
            ViewData["Vehicle"] = vehicle;

            ViewData["ShowBack"] = true;
            ViewData["BackUrl"] = Url.Action("Index");
            ViewData["TopTitle"] = "Tài khoản cá nhân";

            return View(user);
        }

        // GET: /Driver/Incidents
        public async Task<IActionResult> Incidents()
        {
            var user = await _UserManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var incidents = await _context.Incidents
                .Include(i => i.ChuyenDi)
                .Where(i => i.ChuyenDi!.TaiXeId == user.Id)
                .OrderByDescending(i => i.ThoiGian)
                .ToListAsync();

            ViewData["ShowBack"] = true;
            ViewData["BackUrl"] = Url.Action("Index");
            ViewData["TopTitle"] = "Sự cố của tôi";

            return View(incidents);
        }

        // GET: /Driver/ReportIncident/5
        public async Task<IActionResult> ReportIncident(int tripId)
        {
            var user = await _UserManager.GetUserAsync(User);
            var trip = await _context.Trips
                .Include(t => t.Xe)
                .FirstOrDefaultAsync(t => t.MaChuyenDi == tripId && t.TaiXeId == user!.Id);

            if (trip == null) return NotFound();

            var model = new IncidentFormViewModel
            {
                MaChuyenDi = tripId,
                TripInfo = $"#CD{tripId:D3} - {trip.Xe?.BienSo}"
            };

            ViewData["ShowBack"] = true;
            ViewData["BackUrl"] = Url.Action("TripDetail", new { id = tripId });
            ViewData["TopTitle"] = "Báo cáo sự cố";

            return View(model);
        }

        // POST: /Driver/ReportIncident
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportIncident(IncidentFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ShowBack"] = true;
                ViewData["BackUrl"] = Url.Action("TripDetail", new { id = model.MaChuyenDi });
                ViewData["TopTitle"] = "Báo cáo sự cố";
                return View(model);
            }

            var user = await _UserManager.GetUserAsync(User);
            var trip = await _context.Trips
                .Include(t => t.TripDetails)
                    .ThenInclude(td => td.DonHang)
                .Include(t => t.Xe)
                .FirstOrDefaultAsync(t => t.MaChuyenDi == model.MaChuyenDi && t.TaiXeId == user!.Id);

            if (trip == null) return NotFound();

            var incident = new Incident
            {
                MaChuyenDi = model.MaChuyenDi,
                LoaiSuCo = model.LoaiSuCo,
                MoTa = model.MoTa,
                ViTriLat = model.ViTriLat,
                ViTriLng = model.ViTriLng,
                ThoiGian = DateTime.Now,
                TrangThai = IncidentStatus.DangXuLy
            };

            _context.Incidents.Add(incident);

            // Cập nhật trạng thái chuyến đi thành Hủy
            trip.TrangThai = TripStatus.Huy;
            trip.ThoiGianKetThuc = DateTime.Now;

            // Xử lý các đơn hàng chưa giao
            foreach (var td in trip.TripDetails)
            {
                if (td.TrangThai != TripDetailStatus.DaGiao)
                {
                    // Cập nhật chi tiết chuyến về trạng thái Hủy
                    td.TrangThai = TripDetailStatus.Huy;
                    
                    // Trả lại đơn hàng để Điều hành phân công lại
                    if (td.DonHang != null)
                    {
                        td.DonHang.TrangThai = OrderStatus.ChoXuLy;
                    }
                }
            }

            // Chuyển xe vào trạng thái bảo trì
            if (trip.Xe != null)
            {
                trip.Xe.TrangThai = VehicleStatus.BaoTri;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã gửi báo cáo sự cố thành công! Chuyến đi đã bị hủy.";
            return RedirectToAction(nameof(Index));
        }
    }
}
