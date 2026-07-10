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
    public class VehicleManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _UserManager;

        public VehicleManagementController(ApplicationDbContext context, UserManager<AppUser> UserManager)
        {
            _context = context;
            _UserManager = UserManager;
        }

        // GET: /VehicleManagement
        public async Task<IActionResult> Index(string? search, VehicleStatus? status, string? loaiXe, int page = 1)
        {
            var user = await _UserManager.GetUserAsync(User);
            var query = _context.Vehicles.Include(v => v.TaiXe)
                .Where(v => v.DieuHanhId == user!.Id)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(v => v.BienSo.ToLower().Contains(search));
            }

            if (status.HasValue)
            {
                query = query.Where(v => v.TrangThai == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(loaiXe))
            {
                query = query.Where(v => v.LoaiXe == loaiXe);
            }

            var orderedQuery = query.OrderBy(v => v.BienSo);
            int pageSize = 10;
            var vehicles = await PaginatedList<Vehicle>.CreateAsync(orderedQuery, page, pageSize);

            // Stats
            var userReq = await _UserManager.GetUserAsync(User);
            ViewBag.TotalVehicles = await _context.Vehicles.CountAsync(v => v.DieuHanhId == userReq!.Id);
            ViewBag.ActiveVehicles = await _context.Vehicles.CountAsync(v => v.DieuHanhId == userReq!.Id && (v.TrangThai == VehicleStatus.SanSang || v.TrangThai == VehicleStatus.DangDi));
            ViewBag.MaintenanceVehicles = await _context.Vehicles.CountAsync(v => v.DieuHanhId == userReq!.Id && v.TrangThai == VehicleStatus.BaoTri);

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.LoaiXe = loaiXe;
            ViewData["CurrentSearch"] = search;
            ViewData["CurrentStatus"] = status;
            ViewData["CurrentLoaiXe"] = loaiXe;
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Quản lý Phương tiện", "")
            };

            return View(vehicles);
        }

        // GET: /VehicleManagement/Create
        public async Task<IActionResult> Create()
        {
            await LoadDriverList();
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Quản lý Phương tiện", ""),
                ("Thêm xe", "")
            };
            return View(new VehicleFormViewModel());
        }

        // POST: /VehicleManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VehicleFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDriverList();
                return View(model);
            }

            var user = await _UserManager.GetUserAsync(User);

            // Kiểm tra biển số trùng trong phạm vi quản lý của mình
            var exists = await _context.Vehicles.AnyAsync(v => v.BienSo == model.BienSo && v.DieuHanhId == user!.Id);
            if (exists)
            {
                ModelState.AddModelError("BienSo", "Biển số xe đã tồn tại trong danh sách của bạn");
                await LoadDriverList();
                return View(model);
            }

            var vehicle = new Vehicle
            {
                BienSo = model.BienSo,
                LoaiXe = model.LoaiXe,
                TrongTai = model.TrongTai,
                TaiXeId = model.TaiXeId,
                TrangThai = model.TrangThai,
                NgayDangKy = DateTime.Now,
                DieuHanhId = user!.Id,
                AdminId = user.AdminId
            };

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã thêm xe \"{model.BienSo}\" thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /VehicleManagement/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _UserManager.GetUserAsync(User);
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.MaXe == id && v.DieuHanhId == user!.Id);
            if (vehicle == null) return NotFound();

            var model = new VehicleFormViewModel
            {
                MaXe = vehicle.MaXe,
                BienSo = vehicle.BienSo,
                LoaiXe = vehicle.LoaiXe,
                TrongTai = vehicle.TrongTai,
                TaiXeId = vehicle.TaiXeId,
                TrangThai = vehicle.TrangThai
            };

            await LoadDriverList();
            ViewData["Breadcrumb"] = new List<(string Text, string Url)>
            {
                ("Quản lý Phương tiện", Url.Action("Index")!),
                ("Chỉnh sửa", "")
            };
            return View(model);
        }

        // POST: /VehicleManagement/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VehicleFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDriverList();
                return View(model);
            }

            var user = await _UserManager.GetUserAsync(User);
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.MaXe == model.MaXe && v.DieuHanhId == user!.Id);
            if (vehicle == null) return NotFound();

            // Kiểm tra biển số trùng (khác với xe hiện tại)
            var exists = await _context.Vehicles.AnyAsync(v => v.BienSo == model.BienSo && v.MaXe != model.MaXe && v.DieuHanhId == user!.Id);
            if (exists)
            {
                ModelState.AddModelError("BienSo", "Biển số xe đã tồn tại trong danh sách của bạn");
                await LoadDriverList();
                return View(model);
            }

            vehicle.BienSo = model.BienSo;
            vehicle.LoaiXe = model.LoaiXe;
            vehicle.TrongTai = model.TrongTai;
            vehicle.TaiXeId = model.TaiXeId;
            vehicle.TrangThai = model.TrangThai;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã cập nhật xe \"{model.BienSo}\" thành công!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /VehicleManagement/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _UserManager.GetUserAsync(User);
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.MaXe == id && v.DieuHanhId == user!.Id);
            if (vehicle == null) return NotFound();

            // Kiểm tra xe có đang trong chuyến đi không
            var hasTrips = await _context.Trips.AnyAsync(t => t.MaXe == id && (t.TrangThai == TripStatus.DangThucHien || t.TrangThai == TripStatus.ChuaBatDau));
            if (hasTrips)
            {
                TempData["Error"] = $"Không thể xóa xe \"{vehicle.BienSo}\" vì đang có chuyến đi!";
                return RedirectToAction(nameof(Index));
            }

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã xóa xe \"{vehicle.BienSo}\" thành công!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDriverList()
        {
            var user = await _UserManager.GetUserAsync(User);
            var drivers = await _context.TaiXes
                .Where(u => u.DieuHanhId == user!.Id)
                .OrderBy(u => u.FullName)
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.FullName + " (" + u.UserName + ")"
                })
                .ToListAsync();

            drivers.Insert(0, new SelectListItem("-- Chưa gán tài xế --", ""));
            ViewBag.DriverList = drivers;
        }
    }
}
