using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartLogistics.Data;
using SmartLogistics.Models;
using SmartLogistics.Helpers;

namespace SmartLogistics.Controllers
{
    [AuthorizeUserType(UserType.DieuHanh)]
    public class TrackingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _UserManager;

        public TrackingController(ApplicationDbContext context, UserManager<AppUser> UserManager)
        {
            _context = context;
            _UserManager = UserManager;
        }

        // GET: /Tracking
        public async Task<IActionResult> Index(string? filter)
        {
            var user = await _UserManager.GetUserAsync(User);
            var query = _context.Vehicles
                .Include(v => v.TaiXe)
                .Include(v => v.Trips.Where(t => t.TrangThai == TripStatus.DangThucHien))
                .Where(v => v.DieuHanhId == user!.Id)
                .AsQueryable();

            if (filter == "active")
                query = query.Where(v => v.TrangThai == VehicleStatus.DangDi);

            var vehicles = await query.OrderByDescending(v => v.TrangThai == VehicleStatus.DangDi)
                                       .ThenBy(v => v.BienSo)
                                       .ToListAsync();

            // Get latest locations for active trips within managed vehicles
            var vehicleIds = vehicles.Select(v => v.MaXe).ToList();
            var activeTrips = await _context.Trips
                .Where(t => t.TrangThai == TripStatus.DangThucHien && t.DieuHanhId == user!.Id)
                .Select(t => t.MaChuyenDi)
                .ToListAsync();

            var latestLocations = new Dictionary<int, TripLocation>();
            foreach (var tripId in activeTrips)
            {
                var loc = await _context.TripLocations
                    .Where(l => l.MaChuyenDi == tripId)
                    .OrderByDescending(l => l.ThoiGian)
                    .FirstOrDefaultAsync();
                if (loc != null) latestLocations[tripId] = loc;
            }

            ViewBag.Filter = filter;
            ViewBag.LatestLocations = latestLocations;
            ViewBag.ActiveCount = vehicles.Count(v => v.TrangThai == VehicleStatus.DangDi);
            ViewBag.TotalCount = vehicles.Count;

            return View(vehicles);
        }
    }
}
