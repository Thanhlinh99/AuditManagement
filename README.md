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

Sau khi chạy thành công, API sẽ chạy tại địa chỉ: `https://localhost:5001` hoặc `http://localhost:5000`

### 6. Kiểm Tra API

1. Mở trình duyệt và truy cập Swagger UI:
   - `https://localhost:5001/swagger` hoặc
   - `http://localhost:5000/swagger`

2. Thử các API endpoints:
   - POST /api/Product/create
   - PUT /api/Product/update/{id}
   - DELETE /api/Product/delete/{id}

### 7. Kiểm Tra Audit Log

1. Mở SQL Server Management Studio
2. Chạy câu lệnh SQL để kiểm tra log:
```sql
USE AuditManagement;
SELECT * FROM AuditLog ORDER BY Timestamp DESC;
```

## Xử Lý Lỗi Thường Gặp

1. **Lỗi Connection String**:
   - Kiểm tra server name, database name, username và password
   - Đảm bảo SQL Server đang chạy
   - Kiểm tra quyền truy cập của user

2. **Lỗi Migration**:
   - Xóa thư mục Migrations nếu cần
   - Chạy lại `dotnet ef migrations add InitialCreate`
   - Kiểm tra các model class có đúng cấu trúc không

3. **Lỗi Build**:
   - Kiểm tra version của .NET SDK
   - Kiểm tra các package reference trong .csproj
   - Clean solution và build lại

4. **Lỗi Runtime**:
   - Kiểm tra logs trong console
   - Kiểm tra connection string
   - Kiểm tra các service đã được đăng ký trong Program.cs

## Các Tính Năng

- Tự động ghi lại các thao tác thêm, sửa, xóa dữ liệu
- Lưu thông tin chi tiết về người thực hiện, thời gian, và nội dung thay đổi
- Hỗ trợ theo dõi thay đổi cho nhiều bảng dữ liệu

## Cấu Trúc Dự Án

```
AuditManagement/
├── Controllers/         # Các API endpoints
├── Data/               # Database context và cấu hình
├── Models/             # Các model class
├── Audit/              # Các class liên quan đến audit
└── Program.cs          # Cấu hình ứng dụng
```

## Hướng Dẫn Mở Rộng Hệ Thống Audit

### 1. Thêm Bảng Mới Cần Theo Dõi Audit

Để thêm một bảng mới vào hệ thống audit, bạn cần:

1. Tạo model class trong thư mục `Models/`:
```csharp
public class YourNewEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    // Các properties khác
}
```

2. Thêm DbSet vào `ApplicationDbContext`:
```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<YourNewEntity> YourNewEntities { get; set; }
    // Các DbSet khác
}
```

3. Tạo migration và cập nhật database:
```bash
dotnet ef migrations add AddYourNewEntity
dotnet ef database update
```

### 2. Tích Hợp Với Hệ Thống User

Nếu bạn đã có hệ thống User, để thay thế "Anonymous" bằng UserId thực tế:

1. Đảm bảo `IHttpContextAccessor` đã được đăng ký trong `Program.cs`:
```csharp
builder.Services.AddHttpContextAccessor();
```

2. Sửa `ApplicationDbContext` để lấy UserId từ HttpContext:
```csharp
public class ApplicationDbContext : DbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IHttpContextAccessor httpContextAccessor) 
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private string GetCurrentUserId()
    {
        // Lấy UserId từ HttpContext
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userId ?? "Anonymous";
    }

    public override int SaveChanges()
    {
        var userId = GetCurrentUserId();
        // Sử dụng userId trong quá trình tạo audit log
        // ...
    }
}
```

### 3. Cấu Hình Audit Log

1. Các trường trong bảng `AuditLog`:
```csharp
public class AuditLog
{
    public int Id { get; set; }
    public string UserId { get; set; }        // ID của người thực hiện
    public string Action { get; set; }        // Create, Update, Delete
    public string TableName { get; set; }     // Tên bảng bị thay đổi
    public string RecordId { get; set; }      // ID của bản ghi bị thay đổi
    public string Changes { get; set; }       // Nội dung thay đổi (JSON)
    public DateTime Timestamp { get; set; }   // Thời gian thực hiện
    public string IpAddress { get; set; }     // Địa chỉ IP
    public string UserAgent { get; set; }     // Thông tin trình duyệt
}
```

2. Các bảng đang được theo dõi audit:
- Product
- Customer
- Order

### 4. Ví Dụ Thêm Bảng Mới

Giả sử bạn muốn thêm bảng `Category`:

1. Tạo model:
```csharp
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}
```

2. Thêm vào DbContext:
```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<Category> Categories { get; set; }
}
```

3. Tạo migration:
```bash
dotnet ef migrations add AddCategory
dotnet ef database update
```

Sau khi thực hiện các bước trên, hệ thống sẽ tự động theo dõi và ghi lại mọi thay đổi trên bảng `Category`.

### 5. Kiểm Tra Audit Log

Để kiểm tra các thay đổi đã được ghi lại:

```sql
-- Xem tất cả audit log
SELECT * FROM AuditLog ORDER BY Timestamp DESC;

-- Xem audit log của một bảng cụ thể
SELECT * FROM AuditLog WHERE TableName = 'Category' ORDER BY Timestamp DESC;

-- Xem audit log của một người dùng cụ thể
SELECT * FROM AuditLog WHERE UserId = 'user123' ORDER BY Timestamp DESC;
```

## Lưu Ý

1. Hệ thống sẽ tự động ghi lại tất cả các thay đổi dữ liệu
2. Audit log được tạo tự động, không cần thêm code xử lý
3. Đảm bảo đã đăng ký `IHttpContextAccessor` nếu muốn lấy thông tin người dùng
4. Có thể tùy chỉnh thông tin được lưu trong audit log bằng cách sửa đổi `AuditLog` model 