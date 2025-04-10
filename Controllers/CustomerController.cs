using Microsoft.AspNetCore.Mvc;
using AuditManagement.Models;
using AuditManagement.Data;

[Route("api/[controller]")]
[ApiController]
public class CustomerController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CustomerController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create()
    {
        var customer = new Customer
        {
            FullName = "Khách test",
            Email = "test@example.com",
            Phone = "0123456789",
            Orders = new List<Order>()
        };

        _context.Customer.Add(customer);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Đã thêm customer", customer.Id });
    }

    [HttpPut("update/{id}")]
    public async Task<IActionResult> Update(int id)
    {
        var customer = await _context.Customer.FindAsync(id);
        if (customer == null) return NotFound();

        customer.Email = "new@example.com";
        await _context.SaveChangesAsync();

        return Ok("Đã cập nhật customer");
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await _context.Customer.FindAsync(id);
        if (customer == null) return NotFound();

        _context.Customer.Remove(customer);
        await _context.SaveChangesAsync();

        return Ok("Đã xoá customer");
    }
}