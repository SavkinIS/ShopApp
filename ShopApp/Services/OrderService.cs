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

    public async Task<(List<OrderClient> Orders, int TotalOrders, int TotalPages, int CurrentPage)> GetUserOrdersAsync(string userId, int page = 1, int pageSize = 5)
    {
        var response = await _httpClient.GetAsync($"api/orders/user/{userId}?page={page}&pageSize={pageSize}");
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to retrieve user orders: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<PagedOrderResponse>();
        return (result.Orders, result.TotalOrders, result.TotalPages, result.CurrentPage);
    }

    public async Task<OrderClient?> GetOrderAsync(int orderId)
    {
        var response = await _httpClient.GetAsync($"api/orders/{orderId}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<OrderClient>();
        }

        var error = await response.Content.ReadAsStringAsync();
        throw new Exception($"Failed to retrieve order: {error}");
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

    public async Task<List<OrderItemClient>> GetAvailableItemsAsync(List<OrderItemClient> items)
    {
        List<OrderItemClient> result = new List<OrderItemClient>();
        _products = (await _productService.GetProductsAsync()).ToList();

        foreach (var item in items)
        {
            var p = _products.FirstOrDefault(p => p.Id == item.ProductId);
            
            if (p != null && p.Count > 0)
            {
                result.Add(item);
            }
        }

        return result;
    }

    public async Task<int> SaveOrder(OrderClient orderClient)
    {
        var orderDto = new CreateOrderDto
        {
            ClientId = orderClient.ClientId,
            Items = orderClient.Items.Select(item => new CreateOrderItemDto
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                Price = item.Price,
                ProductImageUrl = item.ProductImageUrl
            }).ToList(),
            Total = orderClient.Total,
            Comment = orderClient.Comment,
            Status = orderClient.Status,
            CreatedDate = orderClient.CreatedDate
        };

        Console.WriteLine($"Sending order to server: {System.Text.Json.JsonSerializer.Serialize(orderDto)}");
        var response = await _httpClient.PostAsJsonAsync("api/orders/create", orderDto);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error response from server: {error}");
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

    private class PagedOrderResponse
    {
        public List<OrderClient> Orders { get; set; }
        public int TotalOrders { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}