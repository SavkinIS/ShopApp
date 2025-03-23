using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.JSInterop;
using ShopApp.Models;

public class CartService
{
    private readonly HttpClient _httpClient;
    private readonly ProductService _productService;
    private List<Product> _products = new List<Product>();
    private readonly ILocalStorageService _localStorage;
    private string _userEmail = null;
    private readonly IJSRuntime _jsRuntime;

    public CartService(HttpClient httpClient, ProductService productService, ILocalStorageService localStorage, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _productService = productService;
        _localStorage = localStorage;
        _jsRuntime = jsRuntime;
    }
    
    
    public void SaveCart()
    {
        var json = JsonSerializer.Serialize(Items);
        _jsRuntime.InvokeVoidAsync("localStorage.setItem", "cart", json);
    }

    public async Task LoadCartAsyncToLocalStorage()
    {
        var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "cart");
        if (!string.IsNullOrEmpty(json))
        {
            Items = JsonSerializer.Deserialize<List<CartItem>>(json);
        }
    }

    public async Task AddToCart(Product product, int quantity = 1)
    {
        var existingItem = Items.FirstOrDefault(i => i.ProductId == product.Id);
        if (_userEmail == null)
            _userEmail = await _localStorage.GetItemAsync<string>("userEmail");
        
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
            SaveOnServer(existingItem);
        }
        else
        {
            CartItem newItem = new CartItem { ProductId =product.Id, Quantity = quantity, UserEmail = _userEmail };
            Items.Add(newItem);
            // Сохраняем изменения на сервере
            SaveOnServer(newItem);
        }
        
        SaveCart();
    }
    public List<CartItem> Items { get; private set; } = new();

    public  async Task UpdateProducts()
    {
        _products = (await _productService.GetProductsAsync()).ToList();
    }
    
    public async Task UpdateQuantity(int productId, int quantity)
    {
        if (string.IsNullOrEmpty(_userEmail))
            _userEmail = await _localStorage.GetItemAsync<string>("userEmail");
        
        _products = (await _productService.GetProductsAsync()).ToList();
        int maxProductCount = _products.FirstOrDefault(p => p.Id == productId).Count;
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        
        if (item != null)
        {
            item.Quantity += quantity;
            if (item.Quantity <= 0)
            {
                Items.Remove(item);
            }
            else
            {
                int count = Math.Min(item.Quantity, maxProductCount);
                item.Quantity = count;
            }
          
            // Сохраняем изменения на сервере
            await SaveOnServer(item);
        }
        else if (quantity > 0)
        {
            int count = Math.Min(quantity, maxProductCount);
            CartItem newItem = new CartItem { ProductId = productId, Quantity = count, UserEmail = _userEmail };
            Items.Add(newItem);
            // Сохраняем изменения на сервере
            await SaveOnServer(newItem);
        }

        SaveCart();
    }

    private async Task SaveOnServer(CartItem item)
    {
        Console.WriteLine("try update on Server");
        await _httpClient.PostAsJsonAsync("api/cart", item);
    }

    public async Task ClearCart()
    {
        _userEmail = await _localStorage.GetItemAsync<string>("userEmail");
        Items.Clear();
        await _httpClient.DeleteAsync($"api/cart/{_userEmail}");
        SaveCart();
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
        if (_userEmail == null)
            _userEmail = await _localStorage.GetItemAsync<string>("userEmail");

        if (_userEmail == null)
            await LoadCartAsyncToLocalStorage();
        else
        {
            Items = await _httpClient.GetFromJsonAsync<List<CartItem>>($"api/cart/{_userEmail}") ?? new List<CartItem>();
        }
        
        // Обновляем список продуктов для отображения
        _products = (await _productService.GetProductsAsync()).ToList();
        SaveCart();
    }

    
    
    public async Task RemoveFromCart(int productId)
    {
        if (_userEmail == null)
            _userEmail = await _localStorage.GetItemAsync<string>("userEmail");
        
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            item.Quantity = 0;
            await _httpClient.PostAsJsonAsync("api/cart", new CartItem
            {
                ProductId = productId,
                Quantity = item.Quantity,
                UserEmail = _userEmail
            });
            Items.Remove(item);
        }

        SaveCart();
    }

    public string GetTotalPriceString()
    {
        return GetTotalPrice().ToString("C");
    }

    public async Task LoadUser()
    {
        if (_userEmail == null)
            _userEmail = await _localStorage.GetItemAsync<string>("userEmail");
    }

    public int GetProductsCount(Product product)
    {
       var item =  Items.Where(i => i.ProductId == product.Id).FirstOrDefault();
       if (item != null)
           return item.Quantity;
       else return 0;
    }

}