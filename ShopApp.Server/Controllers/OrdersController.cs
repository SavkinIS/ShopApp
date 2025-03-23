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

        // Проверяем, что заказ создаётся для текущего пользователя
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null || userId != order.ClientId.ToString())
        {
            return Unauthorized("You can only create orders for yourself.");
        }

        // Устанавливаем даты и статус
        order.CreatedDate = DateTime.UtcNow;
        order.Status = "Pending";

        // Сохранение заказа в базе данных
        _context.Orders.Add(order);
        _context.SaveChanges();

        return Ok(new { message = "Order created successfully.", orderId = order.Id });
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
                    i.ProductImageUrl,
                    TotalPrice = i.Quantity * i.Price
                }).ToList()
            })
            .FirstOrDefault();

        if (order == null)
        {
            return NotFound("Order not found.");
        }

        // Проверяем, что пользователь имеет доступ к заказу
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

        // Проверяем, что пользователь запрашивает свои заказы или является администратором
        if (!isAdmin && currentUserId != userId)
        {
            return Unauthorized("You can only view your own orders.");
        }

        var orders = _context.Orders
            .Where(o => o.ClientId.ToString() == userId)
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
}