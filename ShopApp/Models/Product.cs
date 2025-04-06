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
    public float WeightGramm { get; set; }
    public string Color { get; set; } = string.Empty;
    public int Count { get; set; }
    public string? ImageUrl { get; set; } = string.Empty;
    public string? ImageUrl2 { get; set; } = string.Empty;
    public string? ImageUrl3 { get; set; } = string.Empty;
    public string? ImageUrl4 { get; set; } = string.Empty;
    
}