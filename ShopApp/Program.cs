using Blazored.LocalStorage;
using Blazored.Toast;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ShopApp;
using ShopApp.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Устанавливаем базовый адрес API
var apiBaseAddress = "http://localhost:5243";

// Регистрируем глобальный HttpClient с правильным базовым адресом
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiBaseAddress)
});

// Регистрируем HttpClient для OrderService с правильным базовым адресом
builder.Services.AddHttpClient<OrderService>(client =>
{
    client.BaseAddress = new Uri(apiBaseAddress);
}).AddHttpMessageHandler<CustomAuthorizationMessageHandler>();

// Регистрируем HttpClient для ProductService
builder.Services.AddHttpClient<ProductService>(client =>
{
    client.BaseAddress = new Uri(apiBaseAddress);
});

// Регистрируем другие сервисы
builder.Services.AddScoped<CustomAuthorizationMessageHandler>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<PageControlService>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazoredToast();

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CustomAuthenticationStateProvider>());

await builder.Build().RunAsync();