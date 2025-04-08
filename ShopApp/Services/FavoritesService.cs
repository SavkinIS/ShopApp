using System.Net.Http.Json;
using ShopApp.Models;

namespace ShopApp.Services;

public class FavoritesService
{
    private readonly HttpClient _httpClient;
    private List<Product> _favorites = new();

    public event Action OnFavoritesChanged;

    public FavoritesService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task InitializeAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/favorites");
            if (response.IsSuccessStatusCode)
            {
                var favorites = await response.Content.ReadFromJsonAsync<List<Product>>();
                _favorites = favorites ?? new List<Product>();
            }
            else
            {
                _favorites = new List<Product>(); // Если запрос не удался, инициализируем пустой список
            }
        }
        catch (Exception)
        {
            _favorites = new List<Product>(); // Обработка ошибок
        }
    }

    public async Task<List<Product>> GetFavoritesAsync()
    {
        if (_favorites == null || !_favorites.Any())
        {
            await InitializeAsync();
        }
        return _favorites;
    }

    public async Task<bool> CheckIsFavoriteAsync(int productId)
    {
        // Сначала проверяем локальный список
        if (_favorites.Any(p => p.Id == productId))
        {
            return true;
        }

        // Если в локальном списке нет, делаем запрос к серверу
        try
        {
            var response = await _httpClient.GetAsync($"api/favorites/isFavorite/{productId}");
            if (response.IsSuccessStatusCode)
            {
                var isFavorite = await response.Content.ReadFromJsonAsync<bool>();
                if (isFavorite && !_favorites.Any(p => p.Id == productId))
                {
                    // Если товар в избранном на сервере, но его нет в локальном списке, обновляем список
                    var product = await _httpClient.GetFromJsonAsync<Product>($"api/products/{productId}");
                    if (product != null)
                    {
                        _favorites.Add(product);
                    }
                }
                return isFavorite;
            }
        }
        catch (Exception)
        {
            return false;
        }
        return false;
    }

    public async Task AddToFavoritesAsync(Product product)
    {
        // Проверяем, есть ли товар в избранном на сервере
        if (await CheckIsFavoriteAsync(product.Id))
        {
            return; // Если товар уже в избранном, ничего не делаем
        }

        var response = await _httpClient.PostAsJsonAsync("api/favorites/add", product.Id);
        if (response.IsSuccessStatusCode)
        {
            _favorites.Add(product);
            OnFavoritesChanged?.Invoke();
        }
        else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            // Можно перенаправить на страницу логина
        }
    }

    public async Task RemoveFromFavoritesAsync(int productId)
    {
        var product = _favorites.FirstOrDefault(p => p.Id == productId);
        if (product != null)
        {
            var response = await _httpClient.DeleteAsync($"api/favorites/remove/{productId}");
            if (response.IsSuccessStatusCode)
            {
                _favorites.Remove(product);
                OnFavoritesChanged?.Invoke();
            }
        }
    }

    public bool IsFavorite(int productId)
    {
        return _favorites.Any(p => p.Id == productId);
    }
}