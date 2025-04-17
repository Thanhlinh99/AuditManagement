using Microsoft.EntityFrameworkCore;
using AuditManagement.Data;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Http;
using AuditManagement.Services;

var builder = WebApplication.CreateBuilder(args);

// 👉 Đăng ký DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 👉 Cho phép đọc HttpContext trong các service
builder.Services.AddHttpContextAccessor();

// 👉 Đăng ký AuditService
builder.Services.AddScoped<IAuditService, AuditService>();

// Add các services khác nếu cần
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.WebHost.UseKestrel(options =>
{
    options.ListenAnyIP(5000);
});

// Cấu hình logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure Forwarded Headers
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    // Cấu hình để tin tưởng tất cả các proxy
    ForwardLimit = null,
    KnownProxies = { System.Net.IPAddress.Parse("0.0.0.0") }
});

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = string.Empty; // Swagger UI ở http://localhost:5000
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Extension method để lấy IP từ header X-Forwarded-For
public static class HttpContextExtensions
{
    public static string GetClientIP(this HttpContext context)
    {
        // Thử lấy từ X-Forwarded-For trước
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // Lấy IP đầu tiên trong danh sách (IP thực của client)
            var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var ip = ips[0].Trim();
            
            // Xử lý trường hợp IPv6-mapped IPv4
            if (ip.StartsWith("::ffff:"))
            {
                return ip.Substring(7); // Bỏ phần ::ffff: để lấy IPv4
            }
            return ip;
        }

        // Nếu không có X-Forwarded-For, lấy IP trực tiếp
        var remoteIp = context.Connection.RemoteIpAddress;
        if (remoteIp != null)
        {
            // Xử lý trường hợp IPv6-mapped IPv4
            if (remoteIp.IsIPv4MappedToIPv6)
            {
                return remoteIp.MapToIPv4().ToString();
            }
            return remoteIp.ToString();
        }

        return "Unknown";
    }
}