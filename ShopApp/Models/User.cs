namespace ShopApp.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class AuthenticatedUser
{
    public string UserName { get; set; }
    public string Email { get; set; }
}


public class LoginResult
{
    public string Token { get; set; } // Основной токен
    public string RefreshToken { get; set; } // Токен для обновления (если используется)
    public DateTime Expiration { get; set; } // Дата и время истечения токена

    // Дополнительная информация о пользователе
    public string UserId { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public List<string> Roles { get; set; } // Список ролей пользователя
}
