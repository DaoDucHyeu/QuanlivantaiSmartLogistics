using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartLogistics.Data;
using SmartLogistics.Models;

namespace SmartLogistics.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _UserManager;

        public NotificationController(ApplicationDbContext context, UserManager<AppUser> UserManager)
        {
            _context = context;
            _UserManager = UserManager;
        }

        // GET: /Notification
        public IActionResult Index()
        {
            // Build notifications from system events
            var notifications = new List<NotificationItem>();

            // Recent trip completions
            var recentTrips = _context.Trips
                .Where(t => t.TrangThai == TripStatus.HoanThanh && t.ThoiGianKetThuc > DateTime.Now.AddDays(-7))
                .OrderByDescending(t => t.ThoiGianKetThuc)
                .Take(5)
                .ToList();

            foreach (var trip in recentTrips)
            {
                notifications.Add(new NotificationItem
                {
                    Icon = "bi-check-circle-fill",
                    Color = "success",
                    Title = $"Chuyến đi #CD{trip.MaChuyenDi:D3} hoàn thành",
                    Message = $"Hoàn thành lúc {trip.ThoiGianKetThuc?.ToString("dd/MM HH:mm")}",
                    Time = trip.ThoiGianKetThuc ?? DateTime.Now
                });
            }

            // Recent incidents
            var recentIncidents = _context.Incidents
                .Where(i => i.ThoiGian > DateTime.Now.AddDays(-7))
                .OrderByDescending(i => i.ThoiGian)
                .Take(5)
                .ToList();

            foreach (var incident in recentIncidents)
            {
                var typeName = incident.LoaiSuCo switch
                {
                    IncidentType.ChayXe => "Cháy xe",
                    IncidentType.TaiNan => "Tai nạn",
                    IncidentType.HuHong => "Hư hỏng",
                    _ => "Sự cố"
                };
                notifications.Add(new NotificationItem
                {
                    Icon = "bi-exclamation-triangle-fill",
                    Color = "danger",
                    Title = $"Sự cố: {typeName}",
                    Message = incident.MoTa.Length > 60 ? incident.MoTa.Substring(0, 60) + "..." : incident.MoTa,
                    Time = incident.ThoiGian
                });
            }

            // Recent orders
            var recentOrders = _context.Orders
                .Where(o => o.NgayTao > DateTime.Now.AddDays(-3))
                .OrderByDescending(o => o.NgayTao)
                .Take(5)
                .ToList();

            foreach (var order in recentOrders)
            {
                notifications.Add(new NotificationItem
                {
                    Icon = "bi-box-seam-fill",
                    Color = "info",
                    Title = $"Đơn hàng mới #DH{order.MaDonHang:D3}",
                    Message = $"{order.DiemDi} → {order.DiemDen}",
                    Time = order.NgayTao
                });
            }

            return View(notifications.OrderByDescending(n => n.Time).Take(20).ToList());
        }
    }

    public class NotificationItem
    {
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = "info";
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Time { get; set; }
    }
}
