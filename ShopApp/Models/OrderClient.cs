using System.ComponentModel.DataAnnotations;

namespace ShopApp.Models;

public class OrderClient
{
    public int Id { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public List<OrderItemClient> Items { get; set; } = new List<OrderItemClient>();
    public decimal Total { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime CreatedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
}


public class OrderItemClient
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string ProductImageUrl { get; set; } = string.Empty;

    // Свойство для отображения в UI
    public decimal TotalPrice => Quantity * Price;
}

public class CreateOrderDto
{
    public string ClientId { get; set; } = string.Empty;
    public List<CreateOrderItemDto> Items { get; set; } = new List<CreateOrderItemDto>();
    public decimal Total { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime CreatedDate { get; set; }
}

public class CreateOrderItemDto
{
    [Required]
    public int ProductId { get; set; }
    [Required]
    public string ProductName { get; set; } = string.Empty;
    [Required]
    public int Quantity { get; set; }
    [Required]
    public decimal Price { get; set; }
    public string ProductImageUrl { get; set; } = string.Empty;
}