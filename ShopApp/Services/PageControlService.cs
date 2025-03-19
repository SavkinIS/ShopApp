using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace ShopApp.Services;

public class PageControlService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly NavigationManager _navigationManager;

    public PageControlService(HttpClient httpClient, ILocalStorageService localStorage, NavigationManager navigationManager)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _navigationManager = navigationManager;
        _navigationManager.LocationChanged += OnLocationChanged;
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        // if (e.Location.Contains("Product-item"))
        //     IsMainBannerVisible = true;
        // else
        //     IsMainBannerVisible = false;
        //
    }

    public Action OnChange { get; set; }
    
    private bool _isMainBannerVisible = true;
    
    public bool IsMainBannerVisible 
    { 
        get => _isMainBannerVisible; 
        set 
        {
            if (_isMainBannerVisible != value)
            {
                _isMainBannerVisible = value;
                NotifyStateChanged();
            }
        } 
    }

    private void NotifyStateChanged() => OnChange?.Invoke();

}