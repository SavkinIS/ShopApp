using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using ShopApp.Models;

public class CartService
{
    private readonly HttpClient _httpClient;
    private string _sessionId;
    private readonly ProductService _productService;
    private List<Product> _products = new List<Product>();
    private readonly ILocalStorageService _localStorage;

    public CartService(HttpClient httpClient, ProductService productService, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _productService = productService;
        _localStorage = localStorage;
    }

    public async Task InitializeSessionAsync()
    {
        string sessionId = await _localStorage.GetItemAsStringAsync("CartSessionId");
        if (sessionId != null && !string.IsNullOrEmpty(sessionId))
        {
            _sessionId = sessionId.ToString();
        }
        else
        {
            _sessionId = Guid.NewGuid().ToString();
            await _localStorage.SetItemAsync("CartSessionId", _sessionId);
        }
    }

    

    public void AddToCart(Product product, int quantity = 1)
    {
        var existingItem = Items.FirstOrDefault(i => i.ProductId == product.Id);
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            Items.Add(new CartItem
            {
                ProductId = product.Id,
                Quantity = quantity
            });
        }
    }
    public List<CartItem> Items { get; private set; } = new();

  

    public  async Task UpdateProducts()
    {
        _products = (await _productService.GetProductsAsync()).ToList();
    }
    
    public async Task UpdateQuantity(int productId, int quantity)
    {
        _products = (await _productService.GetProductsAsync()).ToList();
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        
        if (item != null)
        {
            item.Quantity += quantity;
            if (item.Quantity <= 0)
            {
                Items.Remove(item);
            }
        }
        else if (quantity > 0)
        {
            Items.Add(new CartItem { ProductId = productId, Quantity = quantity, SessionId = _sessionId });
        }

        // Сохраняем изменения на сервере
        await _httpClient.PostAsJsonAsync("api/cart", new CartItem
        {
            ProductId = productId,
            Quantity = item?.Quantity ?? quantity,
            SessionId = _sessionId
        });
    }

    public async Task ClearCart()
    {
        Items.Clear();
        await _httpClient.DeleteAsync($"api/cart/{_sessionId}");
    }

    public decimal GetTotalPrice()
    {
        var totalPrice = 0m;

        for (int i = 0; i < Items.Count; i++)
        {
            var price = _products.Where(p => p.Id ==  Items[i].ProductId).FirstOrDefault().Price;
            totalPrice += Items[i].Quantity * price;
        }
        
        return totalPrice;
    }

  
    public async Task<List<OrderItem>> GetItems()
    {
        // Обновляем список продуктов перед использованием
        if (!_products.Any())
        {
            _products = (await _productService.GetProductsAsync()).ToList();
        }

        // Генерация списка для отображения
        return Items.Select(item =>
        {
            var product = _products.FirstOrDefault(p => p.Id == item.ProductId);
            if (product != null)
            {
                return new OrderItem(
                    item.ProductId,
                    product.Name,
                    item.Quantity,
                    product.Price,
                    product.ImageUrl
                );
            }

            return null; // Пропускаем отсутствующие товары
        }).Where(orderItem => orderItem != null).ToList();
    }

    public async Task LoadCartAsync()
    {
        Items = await _httpClient.GetFromJsonAsync<List<CartItem>>($"api/cart/{_sessionId}") ?? new List<CartItem>();

        // Обновляем список продуктов для отображения
        _products = (await _productService.GetProductsAsync()).ToList();
    }

    
    
    public async Task RemoveFromCart(int productId)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            item.Quantity = 0;
            await _httpClient.PostAsJsonAsync("api/cart", new CartItem
            {
                ProductId = productId,
                Quantity = item.Quantity,
                SessionId = _sessionId
            });
            Items.Remove(item);
        }
    }

    public string GetTotalPriceString()
    {
        return GetTotalPrice().ToString("C");
    }
}