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
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to save the order: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<Order>();
        if (result != null)
        {
            Console.WriteLine($"Order saved with ID: {result.Id}");
        }
    }

    public async Task<List<Order>?> GetUserOrdersAsync(object id)
    {
        if (id == null)
            return new List<Order>();

        var response = await _httpClient.GetAsync($"api/orders/user/{id}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<Order>>();
        }

        return new List<Order>(); // Возвращаем пустой список в случае ошибки
    }
}