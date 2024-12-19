namespace ShopApp.Server.Shared.Models;

public class Product1
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string ImageUrl { get; set; }
    public string Category { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Weight { get; set; } = string.Empty;
}