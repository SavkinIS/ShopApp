
namespace ShopApp.Models
{
    public class Order
    {
        public int Id { get; set; } // Уникальный идентификатор заказа
        public int ClientId { get; set; } // ID клиента
        public List<OrderItem> Items { get; set; } = new List<OrderItem>(); // Товары в заказе
        public decimal Total { get; set; } // Общая сумма заказа
        public string Comment { get; set; } // Комментарий клиента
        public string Status { get; set; } = "Pending"; // Статус заказа (например, "Pending", "Completed")
        public DateTime CreatedDate { get; set; } = DateTime.Now; // Дата создания
        public DateTime? CompletedDate { get; set; } // Дата завершения (если заказ завершён)
    }

    public class OrderItem
    {
        public OrderItem(int productId, string productName, int quantity, decimal price, string productImageUrl)
        {
            ProductId = productId;
            ProductName = productName;
            Quantity = quantity;
            Price = price;
            ProductImageUrl = productImageUrl;
        }

        public int Id { get; set; } // Уникальный идентификатор
        public int OrderId { get; set; } // Ссылка на заказ
        public int ProductId { get; set; } // ID товара
        public string ProductName { get; set; } // Имя товара
        public int Quantity { get; set; } // Количество
        public decimal Price { get; set; } // Цена за единицу
        public string ProductImageUrl { get; set; }
    }
}
