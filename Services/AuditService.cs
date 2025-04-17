using AuditManagement.Models;
using AuditManagement.Data;
using Microsoft.AspNetCore.Http;
using System;

namespace AuditManagement.Services
{
    public interface IAuditService
    {
        Task LogAsync(string action, string tableName, string recordId, object? oldValues = null, object? newValues = null);
    }

    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string action, string tableName, string recordId, object? oldValues = null, object? newValues = null)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Anonymous";
            var ipAddress = _httpContextAccessor.HttpContext?.GetClientIP() ?? "Unknown";
            var userAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown";

            var log = new AuditLog
            {
                UserId = userId,
                Action = action,
                TableName = tableName,
                RecordId = recordId,
                Changes = System.Text.Json.JsonSerializer.Serialize(new
                {
                    Old = oldValues,
                    New = newValues
                }),
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            _context.AuditLog.Add(log);
            await _context.SaveChangesAsync();
        }
    }
} 