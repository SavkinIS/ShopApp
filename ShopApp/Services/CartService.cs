using ShopApp.Models;

namespace ShopApp.Services;

using System.Collections.Generic;
using System.Linq;

public class CartService
{
    private readonly List<CartItem> _items = new();

    public IEnumerable<CartItem> GetItems() => _items;

    public void AddToCart(Product product, int quantity = 1)
    {
        var existingItem = _items.FirstOrDefault(x => x.Product.Id == product.Id);
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            _items.Add(new CartItem { Product = product, Quantity = quantity });
        }
    }

    public void RemoveFromCart(int productId)
    {
        var item = _items.FirstOrDefault(x => x.Product.Id == productId);
        if (item != null)
        {
            _items.Remove(item);
        }
    }

    public decimal GetTotalPrice() => _items.Sum(x => x.Product.Price * x.Quantity);
}
