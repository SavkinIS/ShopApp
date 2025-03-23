using System.Net.Http.Json;
using ShopApp.Models;

namespace ShopApp.Services;

public class OrderService
{
    private readonly HttpClient _httpClient;
    private readonly ProductService _productService;
    private List<Product> _products;

    public OrderService(HttpClient httpClient, ProductService productService)
    {
        _httpClient = httpClient;
        _productService = productService;
    }

    public async Task<List<Order>> GetUserOrdersAsync(string userId)
    {
        return await _httpClient.GetFromJsonAsync<List<Order>>($"api/orders/user/{userId}") ?? new List<Order>();
    }

    public async Task<Order?> GetOrderAsync(int orderId)
    {
        var response = await _httpClient.GetAsync($"api/orders/{orderId}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Order>();
        }
        return null;
    }

    public async Task CancelOrderAsync(int orderId)
    {
        var response = await _httpClient.PostAsync($"api/orders/cancel/{orderId}", null);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to cancel the order: {error}");
        }
    }

    public async Task<List<OrderItem>> GetAvailableItemsAsync(List<OrderItem> items)
    {
        List<OrderItem> result = new List<OrderItem>();
        _products = (await _productService.GetProductsAsync()).ToList();

        foreach (var item in items)
        {
            var p = _products.Where(p => p.Id == item.ProductId).FirstOrDefault();
            
            if (p != null && p.Count > 0)
            {
                result.Add(item);
            }
        }

        return result;
    }

    public async Task<int> SaveOrder(Order order)
    {
        var response = await _httpClient.PostAsJsonAsync("api/orders/create", order);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to save the order: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<CreateOrderResponse>();
        return result.OrderId;
    }

    private class CreateOrderResponse
    {
        public string Message { get; set; }
        public int OrderId { get; set; }
    }
}