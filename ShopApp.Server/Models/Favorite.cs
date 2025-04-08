using ShopApp.Models;

public class Favorite
{
    public int Id { get; set; }
    public string UserId { get; set; } // ID пользователя
    public ApplicationUser User { get; set; } // Навигационное свойство для пользователя
    public int ProductId { get; set; } // ID товара
    public Product Product { get; set; } // Навигационное свойство для товара
}