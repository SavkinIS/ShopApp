using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopApp.Models;
using ShopApp.Server.Data;

[ApiController]
[Route("api/cart")]
public class CartController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CartController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("{userEmail}")]
    public async Task<IActionResult> GetCart(string userEmail)
    {
        var items = await _context.CartItems
            .Where(c => c.UserEmail == userEmail)
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> AddOrUpdateItem([FromBody] CartItem item)
    {
        var existingItem = await _context.CartItems
            .FirstOrDefaultAsync(c => c.UserEmail == item.UserEmail && c.ProductId == item.ProductId);

        if (existingItem != null)
        {
            existingItem.Quantity = item.Quantity;
            if (existingItem.Quantity <= 0)
            {
                _context.CartItems.Remove(existingItem);
            }
            else
            {
                _context.CartItems.Update(existingItem);
            }
        }
        else if (item.Quantity > 0)
        {
            await _context.CartItems.AddAsync(item);
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{userEmail}")]
    public async Task<IActionResult> ClearCart(string userEmail)
    {
        var items = _context.CartItems.Where(c => c.UserEmail == userEmail);
        _context.CartItems.RemoveRange(items);
        await _context.SaveChangesAsync();
        return Ok();
    }
}