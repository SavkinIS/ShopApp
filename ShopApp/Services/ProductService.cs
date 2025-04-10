using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using OfficeOpenXml;
using ShopApp.Models;

public class ProductService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly IJSRuntime _jsRuntime;

    public ProductService(HttpClient httpClient, ILocalStorageService localStorage, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _jsRuntime = jsRuntime;
    }

    private async Task AddAuthorizationHeader()
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<IEnumerable<Product>> GetProductsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/products");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to fetch products. Status code: {response.StatusCode}");
                return Enumerable.Empty<Product>();
            }

            var products = await response.Content.ReadFromJsonAsync<IEnumerable<Product>>();
            return products ?? Enumerable.Empty<Product>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetProductsAsync: {ex.Message}");
            return Enumerable.Empty<Product>();
        }
    }

    public async Task AddProductAsync(Product product)
    {
        await AddAuthorizationHeader();
        await _httpClient.PostAsJsonAsync("api/products", product);
    }

    public async Task UpdateProductAsync(Product product)
    {
        await AddAuthorizationHeader();
        await _httpClient.PutAsJsonAsync($"api/products/{product.Id}", product);
    }

    public async Task DeleteProductAsync(int productId)
    {
        await AddAuthorizationHeader();
        await _httpClient.DeleteAsync($"api/products/{productId}");
    }

    public async Task<string> UploadImageAsync(IBrowserFile file)
    {
        using var content = new MultipartFormDataContent();
        var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024); // 10MB
        content.Add(new StreamContent(stream), "file", file.Name);

        var response = await _httpClient.PostAsync("api/products/upload", content);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStringAsync();
        Console.WriteLine(result);
        return result;
    }

  

public async Task<Result<List<Product>>> UploadProductsFromExcelAsync(Stream excelStream)
{
    try
    {
        var products = new List<Product>();
        using var package = new ExcelPackage(excelStream);
        var worksheet = package.Workbook.Worksheets[0];

        for (int row = 2; row <= worksheet.Dimension.Rows; row++)
        {
            string? name = worksheet.Cells[row, 1].Text;
            string? type = worksheet.Cells[row, 2].Text;
            decimal price = decimal.Parse(worksheet.Cells[row, 3].Text);
            int count = int.Parse(worksheet.Cells[row, 4].Text);
            string? brand = worksheet.Cells[row, 5].Text;
            string? imageUrl = worksheet.Cells[row, 6].Text;
            string? weight = worksheet.Cells[row, 7].Text;
            string? color = worksheet.Cells[row, 8].Text;

            Product product = type.ToLower() switch
            {
                "tool" => new Tool
                {
                    Material = "",
                    Size = "",
                    Purpose = "",
                    WeightGramm = float.TryParse(weight, out float parsedWeight) ? parsedWeight : 0
                },
                "accessory" => new Accessory
                {
                    Material = "",
                    Size = "",
                    Type = "",
                    WeightGramm = float.TryParse(weight, out float parsedWeight) ? parsedWeight : 0
                },
                "clothing" => new Clothing
                {
                    Size = "",
                    Fabric = "",
                    Season = "",
                    Color = color ?? "",
                    WeightGramm = float.TryParse(weight, out float parsedWeight) ? parsedWeight : 0
                },
                "masterclass" => new MasterClass
                {
                    EventDate = DateTime.Now,
                    DurationHours = 0,
                    DifficultyLevel = "",
                    Format = ""
                },
                "yarnbobbin" => new YarnBobbin // Добавляем YarnBobbin
                {
                    Type = "",
                    Color = color ?? "",
                    WeightGramm = float.TryParse(weight, out float parsedWeight) ? parsedWeight : 0,
                    ToolsSize = "",
                    Length = 0
                },
                "yarn" => new Yarn
                {
                    Type = "",
                    Color = color ?? "",
                    WeightGramm = float.TryParse(weight, out float parsedWeight) ? parsedWeight : 0,
                    ToolsSize = "",
                    Length = 0
                },
                _ => null
            };

            if (product == null)
            {
                continue;
            }

            product.Name = name;
            product.Price = price;
            product.Count = count;
            product.Brand = brand;
            product.ImageUrl = imageUrl;

            products.Add(product);
            await AddProductAsync(product);
        }

        return new Result<List<Product>> { IsSuccess = true, Data = products };
    }
    catch (Exception ex)
    {
        return new Result<List<Product>> { IsSuccess = false, ErrorMessage = "Failed to process Excel file." };
    }
}

public async Task<Stream> DownloadProductsAsExcelAsync()
{
    var products = await GetProductsAsync();

    var stream = new MemoryStream();
    using var package = new ExcelPackage(stream);

    var worksheet = package.Workbook.Worksheets.Add("Products");
    worksheet.Cells[1, 1].Value = "Name";
    worksheet.Cells[1, 2].Value = "Type";
    worksheet.Cells[1, 3].Value = "Price";
    worksheet.Cells[1, 4].Value = "Count";
    worksheet.Cells[1, 5].Value = "Brand";
    worksheet.Cells[1, 6].Value = "ImageUrl";
    worksheet.Cells[1, 7].Value = "WeightGramm";
    worksheet.Cells[1, 8].Value = "Color";

    int row = 2;
    foreach (var product in products)
    {
        worksheet.Cells[row, 1].Value = product.Name;
        worksheet.Cells[row, 2].Value = product switch
        {
            Tool _ => "tool",
            Accessory _ => "accessory",
            Clothing _ => "clothing",
            MasterClass _ => "masterclass",
            YarnBobbin _ => "yarnbobbin", // Добавляем YarnBobbin
            Yarn _ => "yarn",
            _ => "unknown"
        };
        worksheet.Cells[row, 3].Value = product.Price;
        worksheet.Cells[row, 4].Value = product.Count;
        worksheet.Cells[row, 5].Value = product.Brand;
        worksheet.Cells[row, 6].Value = product.ImageUrl;
        if (product is YarnBobbin yarnBobbin)
        {
            worksheet.Cells[row, 7].Value = yarnBobbin.WeightGramm;
        }
        else
        {
            worksheet.Cells[row, 7].Value = product switch
            {
                Tool tool => tool.WeightGramm,
                Accessory accessory => accessory.WeightGramm,
                Clothing clothing => clothing.WeightGramm,
                Yarn yarn => yarn.WeightGramm,
                _ => (float?)null
            };
        }

        if (product is YarnBobbin yarnBobbins)
        {
            worksheet.Cells[row, 8].Value = yarnBobbins.Color;
        }
        worksheet.Cells[row, 8].Value = product switch
        {
            Clothing clothing => clothing.Color,
            Yarn yarn => yarn.Color,
            _ => null
        };

        row++;
    }

    package.Save();
    stream.Position = 0;

    return stream;
}

    public async Task DownloadExcelFile()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/products/download-excel");

            if (response.IsSuccessStatusCode)
            {
                var fileName = "Products.xlsx";
                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                
                await _jsRuntime.InvokeVoidAsync("downloadFile", fileBytes, fileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            }
            else
            {
                Console.WriteLine("Ошибка при получении файла: " + response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    public async Task<Result<List<Product>>> UploadExcelFileAsync(Stream fileStream, string fileName)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StreamContent(fileStream), "file", fileName);

            var response = await _httpClient.PostAsync("api/products/upload-excel", content);
            if (response.IsSuccessStatusCode)
            {
                return new Result<List<Product>> { IsSuccess = true };
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            return new Result<List<Product>> { IsSuccess = false, ErrorMessage = errorMessage };
        }
        catch (Exception ex)
        {
            return new Result<List<Product>> { IsSuccess = false, ErrorMessage = ex.Message };
        }
    }

    public async Task SetProductID(int id)
    {
        await _localStorage.SetItemAsync("productID", id.ToString());
    }

    public async Task<int> GetProductID()
    {
        var res = await _localStorage.GetItemAsync<string>("productID");
        Debug.WriteLine($"ERORR ID ERORR ID  {res}");
        if (!string.IsNullOrEmpty(res))
        {
            return int.Parse(res);
        }

        return -999;
    }

    public async Task SaveSortPreferencesAsync(string sortField, string sortOrder)
    {
        await _localStorage.SetItemAsync("SortField", sortField);
        await _localStorage.SetItemAsync("SortOrder", sortOrder);
    }

    public async Task<(string SortField, string SortOrder)> LoadSortPreferencesAsync()
    {
        var sortField = await _localStorage.GetItemAsync<string>("SortField") ?? "Name";
        var sortOrder = await _localStorage.GetItemAsync<string>("SortOrder") ?? "Ascending";
        return (sortField, sortOrder);
    }

    public async Task DeleteImageAsync(string imageUrl)
    {
        await _httpClient.DeleteAsync($"api/upload?imageUrl={Uri.EscapeDataString(imageUrl)}");
    }
}