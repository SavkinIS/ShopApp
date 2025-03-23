using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopApp.Models;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (model.Password != model.ConfirmPassword)
            return BadRequest("Passwords do not match");

        var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        // Assign "User" role by default
        if (!await _roleManager.RoleExistsAsync("User"))
        {
            await _roleManager.CreateAsync(new IdentityRole("User"));
        }

        await _userManager.AddToRoleAsync(user, "User");

        return Ok("Registration successful");
    }

    [HttpGet("register-admin")]
    public async Task<IActionResult> RegisterAdmin()
    {
        string AdminRole = "Admin";
        string AdminEmail = "admin@example.com";
        string AdminPassword = "Admin123!";

        var userOld = await _userManager.FindByEmailAsync(AdminEmail);
        if (userOld != null)
            return BadRequest("Admin user already exists");

        var user = new ApplicationUser { UserName = "Admin", Email = AdminEmail };
        var result = await _userManager.CreateAsync(user, AdminPassword);

        if (!result.Succeeded)
        {
            Console.WriteLine($"User registration failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            return BadRequest(result.Errors);
        }

        if (!await _roleManager.RoleExistsAsync(AdminRole))
        {
            await _roleManager.CreateAsync(new IdentityRole(AdminRole));
        }

        var roleResult = await _userManager.AddToRoleAsync(user, AdminRole);
        if (!roleResult.Succeeded)
        {
            Console.WriteLine($"Failed to add admin role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            return BadRequest(roleResult.Errors);
        }

        Console.WriteLine($"Admin user registered: {user.Email}");
        return Ok("Admin registration successful");
    }

    [Authorize]
    [HttpGet("currentuser")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Unauthorized();
        }

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new AuthenticatedUser
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            FullName = user.UserName, // Здесь можно добавить поле для FullName в ApplicationUser, если нужно
            RegistrationDate = (await _userManager.GetUserAsync(User)).LockoutEnd?.UtcDateTime ?? DateTime.UtcNow,
            // Пример: если в ApplicationUser нет поля для даты регистрации, можно использовать дату создания токена или другое
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
        {
            return Unauthorized("Invalid credentials");
        }

        var roles = await _userManager.GetRolesAsync(user);

        // Генерация токена
        var tokenGenerator = new JwtTokenGenerator(_configuration);
        var token = tokenGenerator.GenerateToken(user.Id, user.Email, roles);

        var result = new LoginResult
        {
            Token = token,
            Expiration = DateTime.UtcNow.AddMinutes(60),
            UserId = user.Id,
            Email = user.Email,
            FullName = user.UserName, // Измените, если есть FirstName и LastName
            Roles = roles.ToList()
        };

        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok("Logout successful");
    }

    [Authorize]
    [HttpPut("update")]
    public async Task<IActionResult> UpdateUser([FromBody] AuthenticatedUser model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        // Валидация номера телефона
        if (!string.IsNullOrEmpty(model.Phone) && !System.Text.RegularExpressions.Regex.IsMatch(model.Phone, @"^\+7[0-9]{10}$"))
        {
            return BadRequest("Phone number must start with +7 and be followed by exactly 10 digits (e.g., +79991234567).");
        }

        user.Email = model.Email;
        user.UserName = model.Email;
        user.FullName = model.FullName;
        user.Phone = model.Phone;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok("Profile updated successfully");
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        var passwordCheck = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
        if (!passwordCheck)
            return BadRequest("Current password is incorrect");

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok("Password changed successfully");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("assign-role")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return NotFound("User not found");

        if (!await _roleManager.RoleExistsAsync(model.Role))
        {
            await _roleManager.CreateAsync(new IdentityRole(model.Role));
        }

        var result = await _userManager.AddToRoleAsync(user, model.Role);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok($"Role '{model.Role}' assigned to user '{model.Email}'");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("isadmin")]
    public async Task<IActionResult> IsAdmin()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(roles.Contains("Admin"));
    }
}
