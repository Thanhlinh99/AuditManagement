# Hệ Thống Quản Lý Audit (Audit Management)

Hệ thống này giúp theo dõi và ghi lại tất cả các thay đổi dữ liệu trong database một cách tự động.

## Yêu Cầu Hệ Thống

1. **Công Cụ Cần Cài Đặt**:
   - [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
   - [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) hoặc [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
   - [Visual Studio Code](https://code.visualstudio.com/) hoặc [Visual Studio](https://visualstudio.microsoft.com/)
   - [Git](https://git-scm.com/downloads)

2. **Các Package NuGet Cần Thiết**:
   - Microsoft.EntityFrameworkCore.SqlServer
   - Microsoft.EntityFrameworkCore.Tools
   - Microsoft.AspNetCore.OpenApi
   - Microsoft.Extensions.Http
   - Microsoft.Extensions.Logging

## Cài Đặt và Chạy Project

### 1. Clone Repository
```bash
git clone [đường dẫn repository]
cd AuditManagement
```

### 2. Cài Đặt Database

1. Mở SQL Server Management Studio (SSMS) hoặc Azure Data Studio
2. Tạo database mới:
```sql
CREATE DATABASE AuditManagement;
```

3. Cập nhật connection string trong `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=AuditManagement;User Id=sa;Password=your_password;TrustServerCertificate=True;"
  }
}
```

### 3. Cài Đặt Dependencies

```bash
# Restore các package NuGet
dotnet restore

# Cài đặt Entity Framework Core tools
dotnet tool install --global dotnet-ef
```

### 4. Tạo và Cập Nhật Database

```bash
# Tạo migration
dotnet ef migrations add InitialCreate

# Cập nhật database
dotnet ef database update
```

### 5. Chạy Project

```bash
# Build project
dotnet build

# Chạy project
dotnet run
```

Sau khi chạy thành công, API sẽ chạy tại địa chỉ: `http://localhost:5000`

### 6. Kiểm Tra API

1. Mở trình duyệt và truy cập Swagger UI:
   - `http://localhost:5000/swagger`

2. Thử các API endpoints:
   - POST /api/Product/create
   - PUT /api/Product/update/{id}
   - DELETE /api/Product/delete/{id}

## Hướng Dẫn Sử Dụng Tính Năng Audit Logging

### 1. Audit Logging Tự Động

Hệ thống tự động ghi lại các thay đổi dữ liệu trên các bảng:
- Product
- Customer
- Order

Khi thực hiện các thao tác Create/Update/Delete, hệ thống sẽ tự động ghi log với các thông tin:
- UserId: ID người thực hiện
- Action: Loại thao tác (Create, Update, Delete)
- TableName: Tên bảng
- RecordId: ID bản ghi
- Changes: Nội dung thay đổi
- Timestamp: Thời gian
- IpAddress: Địa chỉ IP
- UserAgent: Thông tin trình duyệt

### 2. Ghi Log Tùy Chỉnh

Bạn có thể ghi log ở bất kỳ đâu trong code bằng cách:

1. Inject `IAuditService` vào class cần sử dụng:
```csharp
private readonly IAuditService _auditService;

public YourClass(IAuditService auditService)
{
    _auditService = auditService;
}
```

2. Gọi phương thức `LogAsync` để ghi log:
```csharp
await _auditService.LogAsync(
    action: "TÊN_HÀNH_ĐỘNG", // Tên hành động tùy ý
    tableName: "TÊN_BẢNG",    // Tên bảng liên quan
    recordId: "ID_BẢN_GHI",   // ID của bản ghi
    oldValues: object,        // Giá trị cũ (nếu có)
    newValues: object         // Giá trị mới (nếu có)
);
```

### 3. Ví Dụ Sử Dụng

1. Ghi log khi thực hiện thao tác đặc biệt:
```csharp
await _auditService.LogAsync(
    action: "APPROVE_ORDER",
    tableName: "Order",
    recordId: orderId.ToString(),
    newValues: new { Status = "Approved", ApprovedBy = userId }
);
```

2. Ghi log khi có sự kiện quan trọng:
```csharp
await _auditService.LogAsync(
    action: "SYSTEM_EVENT",
    tableName: "System",
    recordId: "0",
    newValues: new { Event = "Backup completed", Timestamp = DateTime.UtcNow }
);
```

3. Ghi log khi có lỗi xảy ra:
```csharp
await _auditService.LogAsync(
    action: "ERROR",
    tableName: "ErrorLog",
    recordId: Guid.NewGuid().ToString(),
    newValues: new { 
        Error = ex.Message,
        StackTrace = ex.StackTrace,
        Timestamp = DateTime.UtcNow 
    }
);
```

### 4. Kiểm Tra Audit Log

1. Truy vấn trực tiếp trong database:
```sql
-- Xem tất cả audit log
SELECT * FROM AuditLog ORDER BY Timestamp DESC;

-- Xem audit log của một bảng cụ thể
SELECT * FROM AuditLog WHERE TableName = 'Product' ORDER BY Timestamp DESC;

-- Xem audit log của một người dùng cụ thể
SELECT * FROM AuditLog WHERE UserId = 'user123' ORDER BY Timestamp DESC;
```

2. Thông qua API:
```http
GET /api/AuditLog
GET /api/AuditLog/table/{tableName}
GET /api/AuditLog/user/{userId}
```

## Cấu Trúc Dự Án

```
AuditManagement/
├── Controllers/         # Các API endpoints
├── Data/               # Database context và cấu hình
├── Models/             # Các model class
├── Services/           # Các service (bao gồm AuditService)
├── Audit/              # Các class liên quan đến audit
└── Program.cs          # Cấu hình ứng dụng
```

## Lưu Ý

1. Hệ thống sẽ tự động ghi lại tất cả các thay đổi dữ liệu
2. Audit log được tạo tự động, không cần thêm code xử lý
3. Đảm bảo đã đăng ký `IHttpContextAccessor` nếu muốn lấy thông tin người dùng
4. Có thể tùy chỉnh thông tin được lưu trong audit log bằng cách sửa đổi `AuditLog` model
5. Khi sử dụng `IAuditService`, bạn có thể ghi log ở bất kỳ đâu trong code
6. Các thông tin như IP, UserAgent, Timestamp sẽ được tự động lưu
7. Có thể tùy chỉnh tên action và nội dung log theo nhu cầu 