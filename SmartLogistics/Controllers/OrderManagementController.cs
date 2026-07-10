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
    public class OrderManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _UserManager;

        public OrderManagementController(ApplicationDbContext context, UserManager<AppUser> UserManager)
        {
            _context = context;
            _UserManager = UserManager;
        }

        // GET: /OrderManagement
        public async Task<IActionResult> Index(string? search, OrderStatus? status, DateTime? fromDate, DateTime? toDate, int page = 1)
        {
            var user = await _UserManager.GetUserAsync(User);
            var query = _context.Orders.Include(o => o.KhachHang)
                .Where(o => o.DieuHanhId == user!.Id)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(o => o.MaDonHang.ToString().Contains(search)
                    || o.KhachHang!.TenKhachHang.ToLower().Contains(search)
                    || o.DiemDi.ToLower().Contains(search)
                    || o.DiemDen.ToLower().Contains(search));
            }

            if (status.HasValue)
                query = query.Where(o => o.TrangThai == status.Value);

            if (fromDate.HasValue)
                query = query.Where(o => o.NgayTao >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(o => o.NgayTao <= toDate.Value.AddDays(1));

            var orderedQuery = query.OrderByDescending(o => o.NgayTao);
            int pageSize = 10;
            var orders = await PaginatedList<Order>.CreateAsync(orderedQuery, page, pageSize);

            // Stats
            var userReq = await _UserManager.GetUserAsync(User);
            ViewBag.PendingCount = await _context.Orders.CountAsync(o => o.DieuHanhId == userReq!.Id && o.TrangThai == OrderStatus.ChoXuLy);
            ViewBag.AssignedCount = await _context.Orders.CountAsync(o => o.DieuHanhId == userReq!.Id && o.TrangThai == OrderStatus.DaGan);
            ViewBag.DeliveringCount = await _context.Orders.CountAsync(o => o.DieuHanhId == userReq!.Id && o.TrangThai == OrderStatus.DangGiao);
            ViewBag.CompletedCount = await _context.Orders.CountAsync(o => o.DieuHanhId == userReq!.Id && o.TrangThai == OrderStatus.HoanThanh);
            ViewBag.CancelledCount = await _context.Orders.CountAsync(o => o.DieuHanhId == userReq!.Id && o.TrangThai == OrderStatus.Huy);

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewData["CurrentSearch"] = search;
            ViewData["CurrentStatus"] = status;
            ViewData["CurrentFromDate"] = fromDate;
            ViewData["CurrentToDate"] = toDate;

            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Quản lý Đơn hàng", "")
            };

            return View(orders);
        }

        // GET: /OrderManagement/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var user = await _UserManager.GetUserAsync(User);
            var order = await _context.Orders
                .Include(o => o.KhachHang)
                .Include(o => o.TripDetails)
                    .ThenInclude(td => td.ChuyenDi)
                .FirstOrDefaultAsync(o => o.MaDonHang == id && o.DieuHanhId == user!.Id);

            if (order == null) return NotFound();

            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Quản lý Đơn hàng", Url.Action("Index")!),
                ($"Chi tiết #DH{order.MaDonHang:D3}", "")
            };

            return View(order);
        }

        // GET: /OrderManagement/Create
        public async Task<IActionResult> Create()
        {
            await LoadCustomerList();
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Quản lý Đơn hàng", ""),
                ("Tạo đơn hàng", "")
            };
            return View(new OrderFormViewModel());
        }

        // POST: /OrderManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCustomerList();
                return View(model);
            }

            var user = await _UserManager.GetUserAsync(User);
            var order = new Order
            {
                MaKhachHang = model.MaKhachHang,
                DiemDi = model.DiemDi,
                DiemDiLat = model.DiemDiLat,
                DiemDiLng = model.DiemDiLng,
                DiemDen = model.DiemDen,
                DiemDenLat = model.DiemDenLat,
                DiemDenLng = model.DiemDenLng,
                KhoiLuong = model.KhoiLuong,
                GiaCuoc = model.GiaCuoc,
                NgayGiaoHang = model.NgayGiaoHang,
                GhiChu = model.GhiChu,
                TrangThai = OrderStatus.ChoXuLy,
                NgayTao = DateTime.Now,
                DieuHanhId = user!.Id,
                AdminId = user.AdminId
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã tạo đơn hàng #DH{order.MaDonHang:D3} thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /OrderManagement/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _UserManager.GetUserAsync(User);
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.MaDonHang == id && o.DieuHanhId == user!.Id);
            if (order == null) return NotFound();

            var model = new OrderFormViewModel
            {
                MaDonHang = order.MaDonHang,
                MaKhachHang = order.MaKhachHang,
                DiemDi = order.DiemDi,
                DiemDiLat = order.DiemDiLat,
                DiemDiLng = order.DiemDiLng,
                DiemDen = order.DiemDen,
                DiemDenLat = order.DiemDenLat,
                DiemDenLng = order.DiemDenLng,
                KhoiLuong = order.KhoiLuong,
                GiaCuoc = order.GiaCuoc,
                NgayGiaoHang = order.NgayGiaoHang,
                GhiChu = order.GhiChu
            };

            await LoadCustomerList();
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Quản lý Đơn hàng", Url.Action("Index")!),
                ("Chỉnh sửa", "")
            };
            return View(model);
        }

        // POST: /OrderManagement/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(OrderFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCustomerList();
                return View(model);
            }

            var user = await _UserManager.GetUserAsync(User);
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.MaDonHang == model.MaDonHang && o.DieuHanhId == user!.Id);
            if (order == null) return NotFound();

            order.MaKhachHang = model.MaKhachHang;
            order.DiemDi = model.DiemDi;
            order.DiemDiLat = model.DiemDiLat;
            order.DiemDiLng = model.DiemDiLng;
            order.DiemDen = model.DiemDen;
            order.DiemDenLat = model.DiemDenLat;
            order.DiemDenLng = model.DiemDenLng;
            order.KhoiLuong = model.KhoiLuong;
            order.GiaCuoc = model.GiaCuoc;
            order.NgayGiaoHang = model.NgayGiaoHang;
            order.GhiChu = model.GhiChu;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã cập nhật đơn hàng #DH{order.MaDonHang:D3} thành công!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /OrderManagement/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _UserManager.GetUserAsync(User);
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.MaDonHang == id && o.DieuHanhId == user!.Id);
            if (order == null) return NotFound();

            if (order.TrangThai == OrderStatus.DangGiao)
            {
                TempData["Error"] = "Không thể hủy đơn hàng đang giao!";
                return RedirectToAction(nameof(Index));
            }

            order.TrangThai = OrderStatus.Huy;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã hủy đơn hàng #DH{order.MaDonHang:D3}!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /OrderManagement/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _UserManager.GetUserAsync(User);
            var order = await _context.Orders.Include(o => o.TripDetails).FirstOrDefaultAsync(o => o.MaDonHang == id && o.DieuHanhId == user!.Id);
            if (order == null) return NotFound();

            if (order.TripDetails.Any())
            {
                TempData["Error"] = "Không thể xóa đơn hàng đã được gán vào chuyến đi!";
                return RedirectToAction(nameof(Index));
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã xóa đơn hàng #DH{id:D3} thành công!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadCustomerList()
        {
            var user = await _UserManager.GetUserAsync(User);
            var currentAdminId = user?.AdminId;

            var customers = await _context.Customers
                .Where(c => c.AdminId == currentAdminId)
                .OrderBy(c => c.TenKhachHang)
                .Select(c => new SelectListItem
                {
                    Value = c.MaKhachHang.ToString(),
                    Text = c.TenKhachHang
                })
                .ToListAsync();

            customers.Insert(0, new SelectListItem("-- Chọn khách hàng --", ""));
            ViewBag.CustomerList = customers;
        }
    }
}
