using System.Net.Http.Json;
using ShopApp.Models;

namespace ShopApp.Services;

public class OrderService
{
    private readonly HttpClient _httpClient;

    public OrderService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task SaveOrder(Order order)
    {
        var response = await _httpClient.PostAsJsonAsync("api/orders/create", order);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to save the order.");
        }

        var result = await response.Content.ReadFromJsonAsync<Order>();
        Console.WriteLine($"Order saved with ID: {result.Id}");
    }
}