namespace ShopApp.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Weight { get; set; } = string.Empty;
    public int Count { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}