using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopApp.Models;
using ShopApp.Server.Data;
using System.Security.Claims;

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

    [Authorize]
    [HttpPost]
    [Route("create")]
    public IActionResult CreateOrder([FromBody] Order order)
    {
        if (order == null || !order.Items.Any())
        {
            return BadRequest("Order must have items.");
        }

        if (string.IsNullOrEmpty(order.ClientId))
        {
            return BadRequest("ClientId is required.");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null || userId != order.ClientId)
        {
            return Unauthorized("You can only create orders for yourself.");
        }

        var calculatedTotal = order.Items.Sum(i => i.Quantity * i.Price);
        if (order.Total != calculatedTotal)
        {
            return BadRequest("Order total does not match the sum of item prices.");
        }

        order.CreatedDate = DateTime.UtcNow;
        order.Status = "Processing";
        _context.Orders.Add(order);
        _context.SaveChanges();

        return Ok(new { message = "Order created successfully.", orderId = order.Id });
    }

    [Authorize]
    [HttpPost]
    [Route("update")]
    public IActionResult UpdateOrder([FromBody] Order order)
    {
        if (order == null || !order.Items.Any())
        {
            return BadRequest("Order is empty.");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        var orderDb = _context.Orders.FirstOrDefault(o => o.Id == order.Id);
        if (orderDb == null)
        {
            return NotFound("Order not found.");
        }

        if (!isAdmin && orderDb.ClientId != userId)
        {
            return Unauthorized("You can only update your own orders.");
        }

        var validStatuses = new[] { "Processing", "Assembled", "Delivered", "Cancelled" };
        if (!validStatuses.Contains(order.Status))
        {
            return BadRequest($"Invalid status. Allowed values: {string.Join(", ", validStatuses)}.");
        }

        if (order.Status == "Delivered" && orderDb.Status != "Delivered")
        {
            orderDb.CompletedDate = DateTime.UtcNow;
        }
        else if (order.Status != "Delivered")
        {
            orderDb.CompletedDate = null;
        }

        orderDb.Status = order.Status;
        _context.SaveChanges();

        return Ok(new { message = "Order updated successfully.", orderId = order.Id });
    }

    [Authorize]
    [HttpGet]
    [Route("{id}")]
    public IActionResult GetOrder(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

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
                    i.Id,
                    i.OrderId,
                    i.ProductId,
                    i.ProductName,
                    i.Quantity,
                    i.Price,
                    i.ProductImageUrl
                }).ToList()
            })
            .FirstOrDefault();

        if (order == null)
        {
            return NotFound("Order not found.");
        }

        if (!isAdmin && order.ClientId.ToString() != userId)
        {
            return Unauthorized("You can only view your own orders.");
        }

        return Ok(order);
    }

    [Authorize]
    [HttpGet]
    [Route("user/{userId}")]
    public IActionResult GetUserOrders(string userId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        if (!isAdmin && currentUserId != userId)
        {
            return Unauthorized("You can only view your own orders.");
        }

        var orders = _context.Orders
            .Where(o => o.ClientId.ToString() == userId)
            .OrderByDescending(o => o.CreatedDate)
            .Select(o => new Order
            {
                Id = o.Id,
                ClientId = o.ClientId,
                Status = o.Status,
                CreatedDate = o.CreatedDate,
                CompletedDate = o.CompletedDate,
                Total = o.Total,
                Comment = o.Comment,
                Items = o.Items.Select(i => new OrderItem
                {
                    Id = i.Id,
                    OrderId = i.OrderId,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    Price = i.Price,
                    ProductImageUrl = i.ProductImageUrl
                }).ToList()
            })
            .ToList();

        return Ok(orders);
    }

    [Authorize]
    [HttpPost]
    [Route("cancel/{id}")]
    public IActionResult CancelOrder(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        var order = _context.Orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
        {
            return NotFound("Order not found.");
        }

        if (!isAdmin && order.ClientId.ToString() != userId)
        {
            return Unauthorized("You can only cancel your own orders.");
        }

        if (order.Status == "Delivered" || order.Status == "Cancelled")
        {
            return BadRequest("Cannot cancel an order that is already delivered or cancelled.");
        }

        order.Status = "Cancelled";
        _context.SaveChanges();

        return Ok("Order cancelled successfully.");
    }
}