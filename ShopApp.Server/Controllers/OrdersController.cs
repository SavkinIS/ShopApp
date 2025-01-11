using Microsoft.AspNetCore.Mvc;
using ShopApp.Models;
using ShopApp.Server.Data;

namespace ShopApp.Controllers;

[Route("api/orders")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public OrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [Route("create")]
    public IActionResult CreateOrder([FromBody] Order order)
    {
        if (order == null || !order.Items.Any())
        {
            return BadRequest("Order must have items.");
        }

        // Сохранение заказа в базе данных
        _context.Orders.Add(order);
        _context.SaveChanges();

        return Ok(new { message = "Order created successfully.", orderId = order.Id });
    }

    [HttpGet]
    [Route("{id}")]
    public IActionResult GetOrder(int id)
    {
        var order = _context.Orders
            .Where(o => o.Id == id)
            .Select(o => new
            {
                o.Id,
                o.ClientId,
                o.Status,
                o.CreatedDate,
                o.CompletedDate,
                o.Total,
                o.Comment,
                Items = o.Items.Select(i => new
                {
                    i.ProductId,
                    i.ProductName,
                    i.Quantity,
                    i.Price,
                    i.ProductImageUrl
                })
            })
            .FirstOrDefault();

        if (order == null)
        {
            return NotFound("Order not found.");
        }

        return Ok(order);
    }
}