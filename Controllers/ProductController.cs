using Microsoft.AspNetCore.Mvc;
using AuditManagement.Models;
using AuditManagement.Data;

namespace AuditManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
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
            await _context.SaveChangesAsync(); // Gọi SaveChanges sẽ trigger audit

            return Ok(new { message = "Đã thêm sản phẩm", product.Id });
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product == null) return NotFound();

            product.Price = 1999;
            await _context.SaveChangesAsync();

            return Ok("Đã cập nhật sản phẩm");
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product == null) return NotFound();

            _context.Product.Remove(product);
            await _context.SaveChangesAsync();

            return Ok("Đã xoá sản phẩm");
        }
    }
}
