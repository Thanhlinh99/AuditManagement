using Microsoft.AspNetCore.Mvc;
using AuditManagement.Models;
using AuditManagement.Data;
using AuditManagement.Services;

namespace AuditManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public ProductController(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create()
        {
            var product = new Product
            {
                Name = "Sản phẩm test",
                Price = 999,
                Orders = new List<Order>()
            };

            _context.Product.Add(product);
            await _context.SaveChangesAsync();

            // Ghi log tùy chỉnh
            await _auditService.LogAsync(
                action: "mới tạo ra cái này",
                tableName: "Product",
                recordId: product.Id.ToString(),
                newValues: new { product.Name, product.Price }
            );

            return Ok(new { message = "Đã thêm sản phẩm", product.Id });
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product == null) return NotFound();

            var oldPrice = product.Price;
            product.Price = 1999;
            await _context.SaveChangesAsync();

            // Ghi log tùy chỉnh
            await _auditService.LogAsync(
                action: "UPDATE_PRODUCT_PRICE",
                tableName: "Product",
                recordId: product.Id.ToString(),
                oldValues: new { Price = oldPrice },
                newValues: new { Price = product.Price }
            );

            return Ok("Đã cập nhật sản phẩm");
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product == null) return NotFound();

            // Ghi log trước khi xóa
            await _auditService.LogAsync(
                action: "DELETE_PRODUCT",
                tableName: "Product",
                recordId: product.Id.ToString(),
                oldValues: new { product.Name, product.Price }
            );

            _context.Product.Remove(product);
            await _context.SaveChangesAsync();

            return Ok("Đã xoá sản phẩm");
        }

        [HttpPost("custom-action/{id}")]
        public async Task<IActionResult> CustomAction(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product == null) return NotFound();

            // Thực hiện một số thao tác tùy chỉnh
            // ...

            // Ghi log cho thao tác tùy chỉnh
            await _auditService.LogAsync(
                action: "CUSTOM_ACTION",
                tableName: "Product",
                recordId: product.Id.ToString(),
                newValues: new { Action = "Thực hiện thao tác tùy chỉnh", Timestamp = DateTime.UtcNow }
            );

            return Ok("Đã thực hiện thao tác tùy chỉnh");
        }
    }
}
