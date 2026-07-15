# Quản lý Vận tải và giám sát hành trình (SmartLogistics)
SmartLogistics là giải pháp phần mềm số hóa chuỗi cung ứng khép kín, hỗ trợ quản lý đơn hàng, tối ưu hóa ghép chuyến và giám sát tọa độ xe tải theo thời gian thực (Real-time GPS Tracking).
# Tính năng nổi bật
* **SaaS đa người thuê (Multi-Tenancy)** 
* **Định vị thời gian thực (Real-time GPS):** Áp dụng SignalR WebSocket và Leaflet.js.
* **Tự động hóa rủi ro:** Quy trình đóng băng xe và hủy chuyến tự động khi có sự cố.
* **Tối ưu hóa giao việc:** Cân bằng tải tài xế và gợi ý ghép đơn thông minh.
---
## Yêu cầu hệ thống
* [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
* Microsoft SQL Server (2019 trở lên)
* Visual Studio 2022 (Khuyến nghị)
---
##  Hướng dẫn chạy trên Localhost (Môi trường Dev)
### 1. Clone dự án về máy
git clone https://github.com/DaoDucHyeu/QuanlivantaiSmartLogistics.git
cd SmartLogistics-UTC

### 2. Cấu hình Database
Mở file src/SmartLogistics-UTC/SmartLogistics/appsettings.json và thay đổi chuỗi kết nối DefaultConnection trỏ về SQL Server của bạn:
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=SmartLogisticsDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
### 3.Khởi tạo Cơ sở dữ liệu (Database)
Dự án sử dụng Entity Framework Core Code-First. Bạn cần chạy lệnh để tạo database từ các file Migration đã có sẵn.
Mở Package Manager Console trong Visual Studio
Chọn Default project là SmartLogistics
Chạy lệnh:
Update-Database

### 4.Thêm Dữ liệu mẫu (Seed Data)
Sau khi database SmartLogisticsDB được tạo, bạn cần thêm dữ liệu mẫu để hệ thống có thể hoạt động đầy đủ:

Mở SQL Server Management Studio (SSMS) hoặc tool quản lý SQL.
Kết nối tới database SmartLogisticsDB.
Mở file sql/SeedData.sql có sẵn trong thư mục dự án.
Nhấn Execute (F5) để chạy script và chèn dữ liệu mẫu.
### 5.Chạy dự án
Bây giờ mọi thứ đã sẵn sàng. Bạn có thể:

Mở file solution SmartLogistics-UTC.sln bằng Visual Studio 2022 và nhấn F5 (hoặc nút Run).
Hoặc mở terminal, trỏ vào thư mục chứa file SmartLogistics.csproj và chạy lệnh:
dotnet run
