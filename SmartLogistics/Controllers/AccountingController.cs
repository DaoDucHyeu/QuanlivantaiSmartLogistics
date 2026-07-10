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
    [AuthorizeUserType(UserType.KeToan)]
    public class AccountingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _UserManager;

        public AccountingController(ApplicationDbContext context, UserManager<AppUser> UserManager)
        {
            _context = context;
            _UserManager = UserManager;
        }

        // GET: /Accounting
        public async Task<IActionResult> Index(DateTime? tuNgay, DateTime? denNgay)
        {
            var user = await _UserManager.GetUserAsync(User);
            var adminId = user?.AdminId;

            // Default: last 6 months
            var endDate = denNgay ?? DateTime.Today;
            var startDate = tuNgay ?? endDate.AddMonths(-5).Date.AddDays(1 - endDate.AddMonths(-5).Day);

            // ====== STAT CARDS ======
            var ordersInRange = await _context.Orders
                .Where(o => o.AdminId == adminId && o.NgayTao >= startDate && o.NgayTao <= endDate.AddDays(1))
                .ToListAsync();

            var tripsInRange = await _context.Trips
                .Where(t => t.AdminId == adminId && t.ThoiGianBatDau >= startDate && t.ThoiGianBatDau <= endDate.AddDays(1))
                .ToListAsync();

            var tongDoanhThu = ordersInRange.Sum(o => o.GiaCuoc);
            // Chi phí ước tính = 35% doanh thu (fuel + maintenance + driver salary estimate)
            var chiPhiVanHanh = tongDoanhThu * 0.35m;

            // ====== MONTHLY REVENUE CHART ======
            var monthlyRevenue = new List<MonthlyRevenueData>();
            for (int i = 5; i >= 0; i--)
            {
                var monthDate = DateTime.Today.AddMonths(-i);
                var monthStart = new DateTime(monthDate.Year, monthDate.Month, 1);
                var monthEnd = monthStart.AddMonths(1);

                var revenue = await _context.Orders
                    .Where(o => o.AdminId == adminId && o.NgayTao >= monthStart && o.NgayTao < monthEnd)
                    .SumAsync(o => o.GiaCuoc);

                monthlyRevenue.Add(new MonthlyRevenueData
                {
                    Month = $"T{monthDate.Month}/{monthDate.Year}",
                    Revenue = revenue
                });
            }

            // ====== ORDER STATUS CHART ======
            var orderStatusChart = new OrderStatusChartData
            {
                HoanThanh = await _context.Orders.CountAsync(o => o.AdminId == adminId && o.TrangThai == OrderStatus.HoanThanh),
                DangGiao = await _context.Orders.CountAsync(o => o.AdminId == adminId && o.TrangThai == OrderStatus.DangGiao),
                ChoXuLy = await _context.Orders.CountAsync(o => o.AdminId == adminId && o.TrangThai == OrderStatus.ChoXuLy),
                DaHuy = await _context.Orders.CountAsync(o => o.AdminId == adminId && o.TrangThai == OrderStatus.Huy)
            };

            // ====== MONTHLY DETAIL TABLE ======
            var monthlyDetails = new List<MonthlyDetailRow>();
            for (int i = 5; i >= 0; i--)
            {
                var monthDate = DateTime.Today.AddMonths(-i);
                var monthStart = new DateTime(monthDate.Year, monthDate.Month, 1);
                var monthEnd = monthStart.AddMonths(1);

                var monthOrders = await _context.Orders
                    .Where(o => o.AdminId == adminId && o.NgayTao >= monthStart && o.NgayTao < monthEnd)
                    .ToListAsync();

                var monthTrips = await _context.Trips
                    .Where(t => t.AdminId == adminId && t.ThoiGianBatDau >= monthStart && t.ThoiGianBatDau < monthEnd)
                    .ToListAsync();

                var monthRevenue = monthOrders.Sum(o => o.GiaCuoc);
                var fuelCost = monthRevenue * 0.25m; // estimated fuel cost

                monthlyDetails.Add(new MonthlyDetailRow
                {
                    Thang = $"Tháng {monthDate.Month}/{monthDate.Year}",
                    SoChuyen = monthTrips.Count,
                    SoDon = monthOrders.Count,
                    DoanhThu = monthRevenue,
                    ChiPhiNhienLieu = fuelCost
                });
            }

            // ====== RECENT INCIDENTS ======
            var recentIncidents = await _context.Incidents
                .Include(i => i.ChuyenDi)
                    .ThenInclude(t => t!.Xe)
                .Where(i => i.ChuyenDi!.AdminId == adminId && i.TrangThai == IncidentStatus.DangXuLy)
                .OrderByDescending(i => i.ThoiGian)
                .ToListAsync();

            var model = new AccountingDashboardViewModel
            {
                TongDoanhThu = tongDoanhThu,
                ChiPhiVanHanh = chiPhiVanHanh,
                TongChuyenDi = tripsInRange.Count,
                MonthlyRevenue = monthlyRevenue,
                OrderStatusChart = orderStatusChart,
                MonthlyDetails = monthlyDetails,
                TuNgay = startDate,
                DenNgay = endDate,
                RecentIncidents = recentIncidents
            };

            return View(model);
        }

        // POST: /Accounting/ResolveIncident
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolveIncident(int incidentId)
        {
            var user = await _UserManager.GetUserAsync(User);
            var adminId = user?.AdminId;

            var incident = await _context.Incidents
                .Include(i => i.ChuyenDi)
                .FirstOrDefaultAsync(i => i.MaSuCo == incidentId && i.ChuyenDi!.AdminId == adminId);

            if (incident != null)
            {
                incident.TrangThai = IncidentStatus.DaXuLy;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã đánh dấu xử lý xong sự cố #{incident.MaSuCo}.";
            }

            // Lấy lại các parameter filter nếu có để redirect đúng chỗ
            var tuNgay = Request.Form["tuNgay"].ToString();
            var denNgay = Request.Form["denNgay"].ToString();
            
            return RedirectToAction(nameof(Index), new { tuNgay = tuNgay, denNgay = denNgay });
        }
    }
}
