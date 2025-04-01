using System.ComponentModel.DataAnnotations;
using ShopApp.Models;

namespace ShopApp.Server.Models
{
    public class Order
    {
        public int Id { get; set; } // Id генерируется базой данных, не требуется при создании
        [Required]
        public string ClientId { get; set; } = string.Empty;
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        [Required]
        public decimal Total { get; set; }
        public string Comment { get; set; } = string.Empty;
        [Required]
        public string Status { get; set; } = "Pending";
        [Required]
        public DateTime CreatedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
    }
    
    public class OrderItem
    {
        public int Id { get; set; } // Id генерируется базой данных, не требуется при создании
        public int OrderId { get; set; } // Будет установлен сервером после создания Order
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
    
    public class CreateOrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string ProductImageUrl { get; set; } = string.Empty;
    }


    public class CreateOrderDto
    {
        [Required]
        public string ClientId { get; set; } = string.Empty;
        [Required]
        public List<CreateOrderItemDto> Items { get; set; } = new List<CreateOrderItemDto>();
        [Required]
        public decimal Total { get; set; }
        public string Comment { get; set; } = string.Empty;
        [Required]
        public string Status { get; set; } = "Pending";
        [Required]
        public DateTime CreatedDate { get; set; }
    }
}
