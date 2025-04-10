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
                _favorites = new List<Product>();
            }
        }
        catch (Exception)
        {
            _favorites = new List<Product>();
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
        if (_favorites.Any(p => p.Id == productId))
        {
            return true;
        }

        try
        {
            var response = await _httpClient.GetAsync($"api/favorites/isFavorite/{productId}");
            if (response.IsSuccessStatusCode)
            {
                var isFavorite = await response.Content.ReadFromJsonAsync<bool>();
                if (isFavorite && !_favorites.Any(p => p.Id == productId))
                {
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
        if (await CheckIsFavoriteAsync(product.Id))
        {
            return;
        }

        var response = await _httpClient.PostAsJsonAsync("api/favorites/add", product.Id);
        if (response.IsSuccessStatusCode)
        {
            _favorites.Add(product);
            OnFavoritesChanged?.Invoke();
        }
        else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
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