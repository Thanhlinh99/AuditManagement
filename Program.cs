using Microsoft.EntityFrameworkCore;
using AuditManagement.Data;

var builder = WebApplication.CreateBuilder(args);

// 👉 Đăng ký DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 👉 Cho phép đọc HttpContext trong các service
builder.Services.AddHttpContextAccessor();

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