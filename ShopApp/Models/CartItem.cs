namespace ShopApp.Models;

public class CartItem
{
    public int Id { get; set; } // ID записи в БД
    public int ProductId { get; set; } // ID товара
    public int Quantity { get; set; } // Количество
    public string UserEmail { get; set; }
    
}