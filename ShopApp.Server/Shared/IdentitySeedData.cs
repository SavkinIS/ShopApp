using Microsoft.AspNetCore.Identity;

namespace ShopApp.Server.Shared;

public static class IdentitySeedData
{
    private const string AdminRole = "Admin";
    private const string AdminEmail = "admin@example.com";
    private const string AdminPassword = "Admin123!";

    public static async void Seed(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        // Создаем роль "Admin" при необходимости
        if (!await roleManager.RoleExistsAsync(AdminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(AdminRole));
        }

        // Проверяем, есть ли пользователь с email "admin@example.com"
        var adminUser = await userManager.FindByEmailAsync(AdminEmail);
        if (adminUser == null)
        {
            // Создаем нового пользователя-администратора
            adminUser = new IdentityUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(adminUser, AdminPassword);

            if (result.Succeeded)
            {
                // Назначаем пользователю роль "Admin"
                await userManager.AddToRoleAsync(adminUser, AdminRole);
            }
        }
        else
        {
            Console.WriteLine($"User with email {AdminEmail} already exists.");
        }
    }
   
}