using System.ComponentModel.DataAnnotations;

namespace SmartLogistics.Models.ViewModels
{
    public class AccountingDashboardViewModel
    {
        public decimal TongDoanhThu { get; set; }
        public decimal ChiPhiVanHanh { get; set; }
        public decimal LoiNhuan => TongDoanhThu - ChiPhiVanHanh;
        public int TongChuyenDi { get; set; }

        // Chart data
        public List<MonthlyRevenueData> MonthlyRevenue { get; set; } = new();
        public OrderStatusChartData OrderStatusChart { get; set; } = new();

        // Table data
        public List<MonthlyDetailRow> MonthlyDetails { get; set; } = new();

        // Filter
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
        
        // Incident Data
        public List<Incident> RecentIncidents { get; set; } = new();
    }

    public class MonthlyRevenueData
    {
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
    }

    public class OrderStatusChartData
    {
        public int HoanThanh { get; set; }
        public int DangGiao { get; set; }
        public int ChoXuLy { get; set; }
        public int DaHuy { get; set; }
    }

    public class MonthlyDetailRow
    {
        public string Thang { get; set; } = string.Empty;
        public int SoChuyen { get; set; }
        public int SoDon { get; set; }
        public decimal DoanhThu { get; set; }
        public decimal ChiPhiNhienLieu { get; set; }
        public decimal LoiNhuan => DoanhThu - ChiPhiNhienLieu;
    }
}
