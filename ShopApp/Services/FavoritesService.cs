using Blazored.LocalStorage;
using ShopApp.Models;

namespace ShopApp.Services;

public class FavoritesService
{
    private readonly ILocalStorageService _localStorage;
    private const string FavoritesKey = "favorites";
    private List<Product> _favorites = new();

    public event Action OnFavoritesChanged;

    public FavoritesService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task InitializeAsync()
    {
        _favorites = await _localStorage.GetItemAsync<List<Product>>(FavoritesKey) ?? new List<Product>();
    }

    public async Task<List<Product>> GetFavoritesAsync()
    {
        if (_favorites == null || !_favorites.Any())
        {
            _favorites = await _localStorage.GetItemAsync<List<Product>>(FavoritesKey) ?? new List<Product>();
        }
        return _favorites;
    }

    public async Task AddToFavoritesAsync(Product product)
    {
        if (_favorites.Any(p => p.Id == product.Id))
            return;

        _favorites.Add(product);
        await _localStorage.SetItemAsync(FavoritesKey, _favorites);
        OnFavoritesChanged?.Invoke();
    }

    public async Task RemoveFromFavoritesAsync(int productId)
    {
        var product = _favorites.FirstOrDefault(p => p.Id == productId);
        if (product != null)
        {
            _favorites.Remove(product);
            await _localStorage.SetItemAsync(FavoritesKey, _favorites);
            OnFavoritesChanged?.Invoke();
        }
    }

    public bool IsFavorite(int productId)
    {
        return _favorites.Any(p => p.Id == productId);
    }
}