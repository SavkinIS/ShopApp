namespace ShopApp.Models;

public class CartItem
{
    public int Id { get; set; } // ID записи в БД
    public string SessionId { get; set; } // Идентификатор сессии или пользователя
    public int ProductId { get; set; } // ID товара
    public int Quantity { get; set; } // Количество
}