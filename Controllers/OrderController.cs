using Microsoft.AspNetCore.Mvc;
using AuditManagement.Models;
using AuditManagement.Data;

namespace AuditManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] OrderRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Lấy customer và product từ database
            var customer = await _context.Customer.FindAsync(request.CustomerId);
            var product = await _context.Product.FindAsync(request.ProductId);

            if (customer == null || product == null)
            {
                return BadRequest("Customer hoặc Product không tồn tại");
            }

            var order = new Order
            {
                OrderDate = DateTime.UtcNow,
                CustomerId = customer.Id,
                Customer = customer,
                Products = new List<Product> { product }
            };

            _context.Order.Add(order);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã thêm order", order.Id });
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] OrderRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var order = await _context.Order.FindAsync(id);
            if (order == null) return NotFound();

            var customer = await _context.Customer.FindAsync(request.CustomerId);
            var product = await _context.Product.FindAsync(request.ProductId);

            if (customer == null || product == null)
            {   
                return BadRequest("Customer hoặc Product không tồn tại");
            }

            order.CustomerId = customer.Id;
            order.Customer = customer;
            order.Products = new List<Product> { product };
            order.OrderDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("Đã cập nhật order");
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.Order.FindAsync(id);
            if (order == null) return NotFound();

            _context.Order.Remove(order);
            await _context.SaveChangesAsync();

            return Ok("Đã xoá order");
        }
    }

    public class OrderRequest
    {
        public required int CustomerId { get; set; }
        public required int ProductId { get; set; }
    }
}