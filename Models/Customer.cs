using System.Collections.Generic;

namespace AuditManagement.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string Phone { get; set; }

        // Quan hệ: Một customer có thể có nhiều orders
        public required ICollection<Order> Orders { get; set; }
    }
}
