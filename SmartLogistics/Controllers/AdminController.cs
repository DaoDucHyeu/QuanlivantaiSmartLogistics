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
    [AuthorizeUserType(UserType.Admin)]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _UserManager;

        public AdminController(ApplicationDbContext context, UserManager<AppUser> UserManager)
        {
            _context = context;
            _UserManager = UserManager;
        }

        public async Task<IActionResult> Index()
        {
            var adminId = _UserManager.GetUserId(User);

            var totalStaff = await _UserManager.Users.CountAsync(u => u.AdminId == adminId);
            var totalVehicles = await _context.Vehicles.CountAsync(v => v.AdminId == adminId);
            var activeVehicles = await _context.Vehicles.CountAsync(v => v.AdminId == adminId && (v.TrangThai == VehicleStatus.SanSang || v.TrangThai == VehicleStatus.DangDi));
            var pendingOrders = await _context.Orders.CountAsync(o => o.AdminId == adminId && o.TrangThai == OrderStatus.ChoXuLy);
            var activeTrips = await _context.Trips.CountAsync(t => t.AdminId == adminId && t.TrangThai == TripStatus.DangThucHien);
            var completedOrders = await _context.Orders.CountAsync(o => o.AdminId == adminId && o.TrangThai == OrderStatus.HoanThanh);
            var deliveringOrders = await _context.Orders.CountAsync(o => o.AdminId == adminId && o.TrangThai == OrderStatus.DangGiao);
            var cancelledOrders = await _context.Orders.CountAsync(o => o.AdminId == adminId && o.TrangThai == OrderStatus.Huy);
            var assignedOrders = await _context.Orders.CountAsync(o => o.AdminId == adminId && o.TrangThai == OrderStatus.DaGan);

            var model = new DashboardViewModel
            {
                TotalStaff = totalStaff,
                TotalVehicles = totalVehicles,
                ActiveVehicles = activeVehicles,
                PendingOrders = pendingOrders,
                ActiveTrips = activeTrips,
                CompletedOrders = completedOrders,
                DeliveringOrders = deliveringOrders,
                CancelledOrders = cancelledOrders,
                AssignedOrders = assignedOrders
            };

            return View(model);
        }
    }
}
