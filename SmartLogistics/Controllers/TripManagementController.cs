using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartLogistics.Data;
using SmartLogistics.Models;
using SmartLogistics.Models.ViewModels;
using SmartLogistics.Helpers;

namespace SmartLogistics.Controllers
{
    [AuthorizeUserType(UserType.DieuHanh)]
    public class TripManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _UserManager;

        public TripManagementController(ApplicationDbContext context, UserManager<AppUser> UserManager)
        {
            _context = context;
            _UserManager = UserManager;
        }

        // GET: /TripManagement
        public async Task<IActionResult> Index(string? search, TripStatus? status, int page = 1)
        {
            var user = await _UserManager.GetUserAsync(User);
            var query = _context.Trips
                .Include(t => t.Xe)
                .Include(t => t.TaiXe)
                .Include(t => t.TripDetails)
                .Where(t => t.DieuHanhId == user!.Id)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(t => t.TaiXe!.FullName!.ToLower().Contains(search)
                    || t.Xe!.BienSo.ToLower().Contains(search)
                    || t.MaChuyenDi.ToString().Contains(search));
            }

            if (status.HasValue)
                query = query.Where(t => t.TrangThai == status.Value);

            var orderedQuery = query.OrderByDescending(t => t.ThoiGianBatDau);
            int pageSize = 10;
            var trips = await PaginatedList<Trip>.CreateAsync(orderedQuery, page, pageSize);

            // Stats
            var userReq = await _UserManager.GetUserAsync(User);
            ViewBag.NotStarted = await _context.Trips.CountAsync(t => t.DieuHanhId == userReq!.Id && t.TrangThai == TripStatus.ChuaBatDau);
            ViewBag.InProgress = await _context.Trips.CountAsync(t => t.DieuHanhId == userReq!.Id && t.TrangThai == TripStatus.DangThucHien);
            ViewBag.Completed = await _context.Trips.CountAsync(t => t.DieuHanhId == userReq!.Id && t.TrangThai == TripStatus.HoanThanh);

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewData["CurrentSearch"] = search;
            ViewData["CurrentStatus"] = status;

            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Quản lý Chuyến đi", "")
            };

            return View(trips);
        }

        // GET: /TripManagement/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var user = await _UserManager.GetUserAsync(User);
            var trip = await _context.Trips
                .Include(t => t.Xe)
                .Include(t => t.TaiXe)
                .Include(t => t.TripDetails)
                    .ThenInclude(td => td.DonHang)
                        .ThenInclude(o => o!.KhachHang)
                .FirstOrDefaultAsync(t => t.MaChuyenDi == id && t.DieuHanhId == user!.Id);

            if (trip == null) return NotFound();

            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Quản lý Chuyến đi", Url.Action("Index")!),
                ($"Chi tiết #CD{trip.MaChuyenDi:D3}", "")
            };

            return View(trip);
        }

        // GET: /TripManagement/Create
        public async Task<IActionResult> Create()
        {
            var user = await _UserManager.GetUserAsync(User);
            var availableOrders = await _context.Orders
                    .Include(o => o.KhachHang)
                    .Where(o => o.TrangThai == OrderStatus.ChoXuLy && o.DieuHanhId == user!.Id)
                    .OrderBy(o => o.MaDonHang)
                    .ToListAsync();
            
            // --- THUẬT TOÁN ĐỊNH TUYẾN: GOM CỤM TOẠ ĐỘ ĐA CHIỀU (Haversine Double-Constraints) ---
            var clusters = new List<List<Order>>();
            var pool = availableOrders.ToList();

            while (pool.Any())
            {
                var center = pool.First();
                
                // Xác định 1 cụm gồm tâm và các vệ tinh nếu cách nơi rước hàng <= 2km VÀ cách nơi giao hàng <= 5km
                var cluster = pool.Where(o => 
                    GeoHelper.GetDistance(center.DiemDiLat, center.DiemDiLng, o.DiemDiLat, o.DiemDiLng) <= 2.0 &&
                    GeoHelper.GetDistance(center.DiemDenLat, center.DiemDenLng, o.DiemDenLat, o.DiemDenLng) <= 5.0
                ).ToList();
                
                clusters.Add(cluster);
                pool = pool.Except(cluster).ToList();
            }

            var model = new TripFormViewModel
            {
                AvailableOrders = availableOrders,
                OrderClusters = clusters
            };

            await LoadVehicleAndDriverLists();

            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Quản lý Chuyến đi", ""),
                ("Tạo chuyến đi", "")
            };

            return View(model);
        }

        // POST: /TripManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TripFormViewModel model)
        {
            var user = await _UserManager.GetUserAsync(User);

            var assignedVehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.TaiXeId == model.TaiXeId && v.TrangThai == VehicleStatus.SanSang);
            if (assignedVehicle == null)
            {
                ModelState.AddModelError("TaiXeId", "Tài xế này chưa được gán xe, hoặc xe đang bận/bảo trì!");
            }
            else
            {
                model.MaXe = assignedVehicle.MaXe;
            }

            if (!ModelState.IsValid)
            {
                var availableOrders = await _context.Orders
                    .Include(o => o.KhachHang)
                    .Where(o => o.TrangThai == OrderStatus.ChoXuLy && o.DieuHanhId == user!.Id)
                    .ToListAsync();
                    
                var clusters = new List<List<Order>>();
                var pool = availableOrders.ToList();

                while (pool.Any())
                {
                    var center = pool.First();
                    var cluster = pool.Where(o => 
                        GeoHelper.GetDistance(center.DiemDiLat, center.DiemDiLng, o.DiemDiLat, o.DiemDiLng) <= 2.0 &&
                        GeoHelper.GetDistance(center.DiemDenLat, center.DiemDenLng, o.DiemDenLat, o.DiemDenLng) <= 5.0
                    ).ToList();
                    clusters.Add(cluster);
                    pool = pool.Except(cluster).ToList();
                }

                model.AvailableOrders = availableOrders;
                model.OrderClusters = clusters;
                await LoadVehicleAndDriverLists();
                return View(model);
            }

            var trip = new Trip
            {
                MaXe = model.MaXe.Value,
                TaiXeId = model.TaiXeId,
                ThoiGianBatDau = model.ThoiGianBatDau,
                GhiChu = model.GhiChu,
                TrangThai = TripStatus.ChuaBatDau,
                DieuHanhId = user!.Id,
                AdminId = user.AdminId
            };

            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();

            // Gán đơn hàng vào chuyến
            if (model.SelectedOrderIds.Any())
            {
                var orderSeq = 1;
                foreach (var orderId in model.SelectedOrderIds)
                {
                    var order = await _context.Orders.FindAsync(orderId);
                    if (order != null && order.TrangThai == OrderStatus.ChoXuLy)
                    {
                        _context.TripDetails.Add(new TripDetail
                        {
                            MaChuyenDi = trip.MaChuyenDi,
                            MaDonHang = orderId,
                            ThuTu = orderSeq++,
                            TrangThai = TripDetailStatus.ChuaGiao
                        });

                        order.TrangThai = OrderStatus.DaGan;
                    }
                }
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = $"Đã tạo chuyến đi #CD{trip.MaChuyenDi:D3} với {model.SelectedOrderIds.Count} đơn hàng!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /TripManagement/Start/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(int id)
        {
            var user = await _UserManager.GetUserAsync(User);
            var trip = await _context.Trips.Include(t => t.TripDetails).FirstOrDefaultAsync(t => t.MaChuyenDi == id && t.DieuHanhId == user!.Id);
            if (trip == null) return NotFound();

            if (trip.TrangThai != TripStatus.ChuaBatDau)
            {
                TempData["Error"] = "Chuyến đi không ở trạng thái chờ bắt đầu!";
                return RedirectToAction(nameof(Details), new { id });
            }

            trip.TrangThai = TripStatus.DangThucHien;
            trip.ThoiGianBatDau = DateTime.Now;

            // Cập nhật trạng thái đơn hàng
            foreach (var td in trip.TripDetails)
            {
                var order = await _context.Orders.FindAsync(td.MaDonHang);
                if (order != null)
                    order.TrangThai = OrderStatus.DangGiao;
                td.TrangThai = TripDetailStatus.DangGiao;
            }

            // Cập nhật trạng thái xe
            var vehicle = await _context.Vehicles.FindAsync(trip.MaXe);
            if (vehicle != null)
                vehicle.TrangThai = VehicleStatus.DangDi;

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã bắt đầu chuyến đi #CD{trip.MaChuyenDi:D3}!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /TripManagement/Complete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var user = await _UserManager.GetUserAsync(User);
            var trip = await _context.Trips.Include(t => t.TripDetails).FirstOrDefaultAsync(t => t.MaChuyenDi == id && t.DieuHanhId == user!.Id);
            if (trip == null) return NotFound();

            if (trip.TrangThai != TripStatus.DangThucHien)
            {
                TempData["Error"] = "Chuyến đi không ở trạng thái đang thực hiện!";
                return RedirectToAction(nameof(Details), new { id });
            }

            trip.TrangThai = TripStatus.HoanThanh;
            trip.ThoiGianKetThuc = DateTime.Now;

            // Cập nhật trạng thái đơn hàng
            foreach (var td in trip.TripDetails)
            {
                var order = await _context.Orders.FindAsync(td.MaDonHang);
                if (order != null)
                    order.TrangThai = OrderStatus.HoanThanh;
                td.TrangThai = TripDetailStatus.DaGiao;
                td.ThoiGianGiao = DateTime.Now;
            }

            // Cập nhật trạng thái xe
            var vehicle = await _context.Vehicles.FindAsync(trip.MaXe);
            if (vehicle != null)
                vehicle.TrangThai = VehicleStatus.SanSang;

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã hoàn thành chuyến đi #CD{trip.MaChuyenDi:D3}!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /TripManagement/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _UserManager.GetUserAsync(User);
            var trip = await _context.Trips.Include(t => t.TripDetails).FirstOrDefaultAsync(t => t.MaChuyenDi == id && t.DieuHanhId == user!.Id);
            if (trip == null) return NotFound();

            if (trip.TrangThai == TripStatus.DangThucHien)
            {
                TempData["Error"] = "Không thể xóa chuyến đi đang thực hiện!";
                return RedirectToAction(nameof(Index));
            }

            // Trả lại trạng thái đơn hàng
            foreach (var td in trip.TripDetails)
            {
                var order = await _context.Orders.FindAsync(td.MaDonHang);
                if (order != null && order.TrangThai == OrderStatus.DaGan)
                    order.TrangThai = OrderStatus.ChoXuLy;
            }

            _context.TripDetails.RemoveRange(trip.TripDetails);
            _context.Trips.Remove(trip);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã xóa chuyến đi #CD{id:D3}!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadVehicleAndDriverLists()
        {
            var user = await _UserManager.GetUserAsync(User);
            var vehicles = await _context.Vehicles
                .Where(v => v.TrangThai == VehicleStatus.SanSang && v.DieuHanhId == user!.Id)
                .OrderBy(v => v.BienSo)
                .Select(v => new SelectListItem
                {
                    Value = v.MaXe.ToString(),
                    Text = v.BienSo + " - " + v.LoaiXe
                })
                .ToListAsync();
            vehicles.Insert(0, new SelectListItem("-- Chọn xe --", ""));
            ViewBag.VehicleList = vehicles;

            // --- TỐI ƯU HOÁ: CHỌN TÀI XẾ THÔNG MINH ---
            var today = DateTime.Today;
            
            // Lấy tất cả lịch sử chuyến đi CỦA NGÀY HÔM NAY để đánh giá độ mệt mỏi
            var tripsToday = await _context.Trips
                .Where(t => t.ThoiGianBatDau.Date == today && t.DieuHanhId == user!.Id)
                .ToListAsync();

            var drivers = await _context.TaiXes
                .Where(u => u.DieuHanhId == user!.Id)
                .ToListAsync();

            var driverItems = new List<SelectListItem>();

            var groupSuggest = new SelectListGroup { Name = "🔥 ĐỀ XUẤT (Đang rảnh: Ưu tiên chia đều việc)" };
            var groupAvailable = new SelectListGroup { Name = "🚗 ĐANG RẢNH (Nhưng hôm nay đã chạy nhiều)" };
            var groupBusy = new SelectListGroup { Name = "⏳ KHÔNG THỂ CHỌN (Đang dở chuyến khác)" };

            foreach (var d in drivers)
            {
                var dTrips = tripsToday.Where(t => t.TaiXeId == d.Id).ToList();
                bool isBusy = dTrips.Any(t => t.TrangThai == TripStatus.DangThucHien);
                int countToday = dTrips.Count;

                var item = new SelectListItem { Value = d.Id };
                
                if (isBusy)
                {
                    item.Text = $"[Đang Bận] {d.FullName} - Chạy {countToday} chuyến h.nay";
                    item.Group = groupBusy;
                    item.Disabled = true; // Không cho phép nhấp vào
                }
                else if (countToday <= 1) // Khuyên chọn người làm ít
                {
                    item.Text = $"[Khuyên Chọn] {d.FullName} - Mới có {countToday} chuyến";
                    item.Group = groupSuggest;
                }
                else
                {
                    item.Text = $"{d.FullName} - Đã cày {countToday} chuyến";
                    item.Group = groupAvailable;
                }
                
                driverItems.Add(item);
            }

            // Sắp xếp: Gợi ý Tốt nhất lên đầu
            driverItems = driverItems.OrderBy(x => x.Group == groupSuggest ? 0 : (x.Group == groupAvailable ? 1 : 2))
                                     .ThenBy(x => x.Text).ToList();

            driverItems.Insert(0, new SelectListItem("-- Chọn tài xế --", ""));
            ViewBag.DriverList = driverItems;
        }
    }
}
