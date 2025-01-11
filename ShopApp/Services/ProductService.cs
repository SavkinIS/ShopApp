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
        await AddAuthorizationHeader();
        return await _httpClient.GetFromJsonAsync<IEnumerable<Product>>("api/products");
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

        var result =response.Content.ReadAsStringAsync();
        Console.WriteLine(result.Result);
        return result.Result;
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
                var product = new Product
                {
                    Name = worksheet.Cells[row, 1].Text,
                    Category = worksheet.Cells[row, 2].Text,
                    Price = decimal.Parse(worksheet.Cells[row, 3].Text),
                    Count = int.Parse(worksheet.Cells[row, 4].Text),
                    Brand = worksheet.Cells[row, 5].Text,
                    ImageUrl = worksheet.Cells[row, 6].Text
                };

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
        worksheet.Cells[1, 2].Value = "Category";
        worksheet.Cells[1, 3].Value = "Price";
        worksheet.Cells[1, 4].Value = "Count";
        worksheet.Cells[1, 5].Value = "Brand";
        worksheet.Cells[1, 6].Value = "ImageUrl";

        int row = 2;
        foreach (var product in products)
        {
            worksheet.Cells[row, 1].Value = product.Name;
            worksheet.Cells[row, 2].Value = product.Category;
            worksheet.Cells[row, 3].Value = product.Price;
            worksheet.Cells[row, 4].Value = product.Count;
            worksheet.Cells[row, 5].Value = product.Brand;
            worksheet.Cells[row, 6].Value = product.ImageUrl;

            row++;
        }

        package.Save();
        stream.Position = 0;

        return stream;
    }
    
    // public async Task DownloadProductsAsExcel(List<Product> products)
    // {
    //     var jsonProducts = JsonSerializer.Serialize(products);
    //     
    //     // Вызов JavaScript функции для скачивания Excel файла
    //     await _jsRuntime.InvokeVoidAsync("downloadExcelFile", jsonProducts);
    // }
    
    
    public async Task DownloadExcelFile()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/products/download-excel");

            if (response.IsSuccessStatusCode)
            {
                var fileName = "Products.xlsx";
                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                
                // Вызов JS для скачивания файла
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
}