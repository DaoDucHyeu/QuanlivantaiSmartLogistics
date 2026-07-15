SmartLogistics-UTC
Hệ thống quản lý SmartLogistics. Dự án được xây dựng với mục tiêu quản lý giao nhận và vận chuyển hàng hóa, sử dụng ASP.NET Core MVC 8.0, Entity Framework Core và MSSQL Server.

🛠 Yêu cầu hệ thống
Trước khi chạy dự án, hãy đảm bảo máy tính của bạn đã cài đặt các phần mềm sau:

.NET 8 SDK
SQL Server (Express hoặc Developer Edition) hoặc sử dụng LocalDB của Visual Studio.
Visual Studio 2022 (Khuyến nghị, hỗ trợ tốt nhất cho .NET 8)
🚀 Hướng dẫn cài đặt và chạy dự án
Bước 1: Clone dự án
Mở terminal hoặc Git Bash và chạy lệnh sau để clone dự án về máy:

git clone https://github.com/DaoDucHyeu/QuanlivantaiSmartLogistics.git
cd SmartLogistics-UTC
Bước 2: Cấu hình chuỗi kết nối (Connection String)
Mở file src/SmartLogistics-UTC/SmartLogistics/appsettings.json và cập nhật DefaultConnection cho phù hợp với SQL Server của bạn. Ví dụ:

"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=SmartLogisticsDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
(Thay thế YOUR_SERVER_NAME bằng tên SQL Server trên máy bạn, ví dụ: localhost, .\SQLEXPRESS hoặc (localdb)\MSSQLLocalDB)

Bước 3: Khởi tạo Cơ sở dữ liệu (Database)
Dự án sử dụng Entity Framework Core Code-First. Bạn cần chạy lệnh để tạo database từ các file Migration đã có sẵn.

Cách 1: Sử dụng Visual Studio (Package Manager Console)

Mở Package Manager Console trong Visual Studio
Chọn Default project là SmartLogistics
Chạy lệnh:
Update-Database
Cách 2: Sử dụng .NET CLI Mở terminal tại thư mục chứa file .csproj (src/SmartLogistics-UTC/SmartLogistics) và chạy:

dotnet ef database update
Bước 4: Thêm Dữ liệu mẫu (Seed Data)
Sau khi database SmartLogisticsDB được tạo, bạn cần thêm dữ liệu mẫu để hệ thống có thể hoạt động đầy đủ:

Mở SQL Server Management Studio (SSMS) hoặc tool quản lý SQL.
Kết nối tới database SmartLogisticsDB.
Mở file sql/SeedData.sql có sẵn trong thư mục dự án.
Nhấn Execute (F5) để chạy script và chèn dữ liệu mẫu.
Bước 5: Chạy dự án
Bây giờ mọi thứ đã sẵn sàng. Bạn có thể:

Mở file solution SmartLogistics-UTC.sln bằng Visual Studio 2022 và nhấn F5 (hoặc nút Run).
Hoặc mở terminal, trỏ vào thư mục chứa file SmartLogistics.csproj và chạy lệnh:
dotnet run
Dự án sẽ khởi chạy và mở trên trình duyệt.
