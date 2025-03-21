using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Blazored.LocalStorage;
using ShopApp.Models;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    public AuthService(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    public async Task<bool> Register(string email, string password, string confirmPassword)
    {
        var result = await _httpClient.PostAsJsonAsync("api/account/register", new { email, password, confirmPassword });
        return result.IsSuccessStatusCode;
    }

    public async Task<bool> Login(string email, string password, bool rememberMe)
    {
        var response = await _httpClient.PostAsJsonAsync("api/account/login", new { email, password, rememberMe });
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResult>();
            if (result != null && !string.IsNullOrEmpty(result.Token))
            {
                await _localStorage.SetItemAsync("authToken", result.Token);
                await _localStorage.SetItemAsync("userEmail", result.Email);
                return true;
            }
        }
        return false;
    }

    public async Task Logout()
    {
        await _httpClient.PostAsync("api/account/logout", null);
        await _localStorage.RemoveItemAsync("authToken");
        await _localStorage.RemoveItemAsync("userEmail");
    }

    public async Task<bool> IsAdmin()
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");
        if (string.IsNullOrEmpty(token)) return false;

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        return jwtToken.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    }

    public async Task<AuthenticatedUser> GetCurrentUserAsync()
    {
        var token = await GetAuthTokenAsync();
        if (string.IsNullOrEmpty(token)) return null;

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _httpClient.GetFromJsonAsync<AuthenticatedUser>("api/account/currentuser");
    }

    public async Task<string> GetAuthTokenAsync()
    {
        return await _localStorage.GetItemAsync<string>("authToken");
    }
}