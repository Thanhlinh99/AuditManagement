using System;
using System.Collections.Generic;

namespace AuditManagement.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }

        public required Customer Customer { get; set; }

        // Một order có thể chứa nhiều sản phẩm (nếu muốn phức tạp hơn, có thể dùng bảng trung gian)
        public required ICollection<Product> Products { get; set; }
    }
}
