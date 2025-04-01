using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Blazored.LocalStorage;
using ShopApp.Models;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private AuthenticatedUser? _cachedUser;
    public event Action OnUserChanged;

    public AuthService(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    public async Task<(bool Success, string ErrorMessage)> Register(string email, string password, string confirmPassword)
    {
        var response = await _httpClient.PostAsJsonAsync("api/account/register", new { email, password, confirmPassword });
        if (response.IsSuccessStatusCode)
        {
            var loginResult = await Login(email, password, false);
            return loginResult;
        }
        var error = await response.Content.ReadAsStringAsync();
        return (false, error);
    }

    public async Task<(bool Success, string ErrorMessage)> Login(string email, string password, bool rememberMe, CustomAuthenticationStateProvider authStateProvider = null)
    {
        var response = await _httpClient.PostAsJsonAsync("api/account/login", new { email, password, rememberMe });
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResult>();
            if (result != null && !string.IsNullOrEmpty(result.Token))
            {
                await _localStorage.SetItemAsync("authToken", result.Token);
                await _localStorage.SetItemAsync("userEmail", result.Email);
                _cachedUser = await FetchCurrentUserAsync();
                if (authStateProvider != null)
                {
                    authStateProvider.NotifyUserAuthentication(result.Token);
                }
                OnUserChanged?.Invoke();
                return (true, null);
            }
        }
        var error = await response.Content.ReadAsStringAsync();
        return (false, error);
    }

    public async Task Logout(CustomAuthenticationStateProvider authStateProvider = null)
    {
        await _httpClient.PostAsync("api/account/logout", null);
        await _localStorage.RemoveItemAsync("authToken");
        await _localStorage.RemoveItemAsync("userEmail");
        _cachedUser = null;
        if (authStateProvider != null)
        {
            authStateProvider.NotifyUserLogout();
        }
        OnUserChanged?.Invoke();
    }

    public async Task<bool> IsAdmin()
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");
        if (string.IsNullOrEmpty(token)) return false;

        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        return jwtToken.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    }

    public async Task<AuthenticatedUser?> GetCurrentUserAsync()
    {
        if (_cachedUser != null)
        {
            return _cachedUser;
        }

        _cachedUser = await FetchCurrentUserAsync();
        return _cachedUser;
    }

    public async Task UpdateUserAsync(AuthenticatedUser user)
    {
        var token = await GetAuthTokenAsync();
        if (string.IsNullOrEmpty(token))
            throw new Exception("User is not authenticated.");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _httpClient.PutAsJsonAsync("api/account/update", user);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to update user: {error}");
        }

        _cachedUser = user;
        OnUserChanged?.Invoke();
    }

    public async Task ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var token = await GetAuthTokenAsync();
        if (string.IsNullOrEmpty(token))
            throw new Exception("User is not authenticated.");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _httpClient.PostAsJsonAsync("api/account/change-password", new { userId, currentPassword, newPassword });
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to change password: {error}");
        }
    }

    public async Task<bool> IsTokenValidAsync()
    {
        var token = await GetAuthTokenAsync();
        if (string.IsNullOrEmpty(token)) return false;

        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        return jwtToken.ValidTo >= DateTime.UtcNow;
    }
    
    public async Task<string> GetAuthTokenAsync()
    {
        return await _localStorage.GetItemAsync<string>("authToken");
    }

    private async Task<AuthenticatedUser?> FetchCurrentUserAsync()
    {
        var token = await GetAuthTokenAsync();
        if (string.IsNullOrEmpty(token)) return null;

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _httpClient.GetAsync("api/account/currentuser");
        if (response.IsSuccessStatusCode)
        {
            var userData = await response.Content.ReadFromJsonAsync<AuthenticatedUser>();
            if (userData != null)
            {
                userData.FullName ??= "Not set";
                userData.Phone ??= string.Empty;
                userData.RegistrationDate = userData.RegistrationDate == default ? DateTime.UtcNow : userData.RegistrationDate;
            }
            return userData;
        }
        return null;
    }
}