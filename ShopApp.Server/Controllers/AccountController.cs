using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    public AccountController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
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

        var user = new IdentityUser { UserName = model.Email, Email = model.Email };
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

        var userOdl = await _userManager.FindByEmailAsync(AdminEmail);
        if (userOdl == null)
            return BadRequest("Invalid email address");

        var user = new IdentityUser { UserName = "Admin", Email = AdminEmail };
        var result = await _userManager.CreateAsync(user, AdminPassword);

        if (!result.Succeeded)
        {
            Console.WriteLine($"User registered: {user.Email}");
            return BadRequest(result.Errors);
        }


        var r = await _userManager.AddToRoleAsync(user, AdminRole);

        if (r.Succeeded)
        {
            Console.WriteLine($"Add admin role: {user.Email}");
        }

        return Ok("Registration successful");
    }

    
    // [HttpGet("currentuser")]
    // public async Task<IActionResult> GetCurrentUser()
    // {
    //     var user = await _userManager.GetUserAsync(User);
    //     if (user == null)
    //     {
    //         return Unauthorized();
    //     }
    //
    //     return Ok(new AuthenticatedUser
    //     {
    //         UserName = user.UserName,
    //         Email = user.Email
    //     });
    // }
    
    [Authorize]
    [HttpGet("currentuser")]
    public  async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        if (userId == null)
        {
            return Unauthorized();
        }
        
        return Ok(new { UserId = userId, Email = email });
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
            FullName = $"{user.UserName}", // Измените, если есть FirstName и LastName
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

public class RegisterModel
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
}

public class LoginModel
{
    public string Email { get; set; }
    public string Password { get; set; }
    public bool RememberMe { get; set; }
}

public class AssignRoleModel
{
    public string Email { get; set; }
    public string Role { get; set; }
}