using ShopApp.Models;

namespace ShopApp.Services;

public class UserService
{
    private readonly List<User> _users = new();
    private static UserService _instance;
    private User? _currentUser;

    public UserService()
    {
        _instance = this;
    }
    public static User? CurrentUser
    {
        get => _instance?._currentUser;
        set => _instance._currentUser = value;
    }

    public bool Register(User user)
    {
        if (_users.Any(u => u.Username == user.Username || u.Email == user.Email))
        {
            return false; // Пользователь с таким именем или email уже существует
        }

        _users.Add(user);
        return true;
    }

    public bool Login(string username, string password)
    {
        var user = _users.FirstOrDefault(u => u.Username == username && u.Password == password);
        if (user != null)
        {
            _currentUser = user;
            return true;
        }
        return false;
    }

    public void Logout()
    {
        _currentUser = null;
    }
}