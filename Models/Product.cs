using System.Collections.Generic;

namespace AuditManagement.Models
{
    public class Product
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public decimal Price { get; set; }

        // Quan hệ: Một product có thể nằm trong nhiều orders
        public required ICollection<Order> Orders { get; set; }
    }
}
