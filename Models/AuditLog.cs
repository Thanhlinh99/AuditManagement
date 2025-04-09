using System;

// Định nghĩa bảng AuditLog
namespace AuditManagement.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public required string UserId { get; set; }
        public required string Action { get; set; } // Create, Update, Delete
        public required string TableName { get; set; }
        public required string RecordId { get; set; }
        public required string Changes { get; set; } // JSON
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public required string IpAddress { get; set; }
        public required string UserAgent { get; set; }
    }
}