using System.ComponentModel.DataAnnotations;
using SmartLogistics.Models;

namespace SmartLogistics.Models.ViewModels
{
    public class DriverDashboardViewModel
    {
        public string DriverName { get; set; } = string.Empty;
        public string? VehiclePlate { get; set; }
        public int TripsToday { get; set; }
        public int InProgressTrips { get; set; }
        public int CompletedTrips { get; set; }
        public int IncidentCount { get; set; }
        public Trip? CurrentTrip { get; set; }
        public List<Trip> UpcomingTrips { get; set; } = new();
    }

    public class IncidentFormViewModel
    {
        public int MaChuyenDi { get; set; }
        public string? TripInfo { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại sự cố")]
        [Display(Name = "Loại sự cố")]
        public IncidentType LoaiSuCo { get; set; }

        [Required(ErrorMessage = "Vui lòng mô tả sự cố")]
        [Display(Name = "Mô tả chi tiết")]
        [StringLength(1000)]
        public string MoTa { get; set; } = string.Empty;

        [Display(Name = "Vĩ độ")]
        public decimal ViTriLat { get; set; }

        [Display(Name = "Kinh độ")]
        public decimal ViTriLng { get; set; }
    }
}
