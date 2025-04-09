// Định nghĩa bảng AuditEntry, đây là 1 class tạm thời, không lưu trong DB dùng để ghi lại
// Ai thao tác gì (UserId), thao tác gì (Action), thao tác trên bảng nào (TableName), ID của bản ghi (RecordId)
// Các giá trị cũ (OldValues), các giá trị mới (NewValues), IP (IpAddress), UserAgent
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AuditManagement.Audit
{
    public class AuditEntry
    {
        public required string UserId { get; set; }
        public required string Action { get; set; }
        public required string TableName { get; set; }
        public required string RecordId { get; set; }
        public Dictionary<string, object> OldValues { get; set; } = new();
        public Dictionary<string, object> NewValues { get; set; } = new();
        public required string IpAddress { get; set; }
        public required string UserAgent { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(new
            {
                Old = OldValues,
                New = NewValues
            });
        }
    }
}