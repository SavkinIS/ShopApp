using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using ShopApp.Server.Data;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/products
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        var products = await _context.Products.ToListAsync();
        return Ok(products);
    }

    // POST: api/products
    [HttpPost]
    public async Task<ActionResult<Product>> AddProduct([FromBody] Product newProduct)
    {
        if (newProduct == null || string.IsNullOrWhiteSpace(newProduct.Name))
        {
            return BadRequest("Invalid product data.");
        }

        _context.Products.Add(newProduct);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProductById), new { id = newProduct.Id }, newProduct);
    }

    // GET: api/products/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProductById(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    // PUT: api/products/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateProduct(int id, [FromBody] Product updatedProduct)
    {
        if (updatedProduct == null || id != updatedProduct.Id)
        {
            return BadRequest("Product ID mismatch or invalid product data.");
        }

        var existingProduct = await _context.Products.FindAsync(id);
        if (existingProduct == null)
        {
            return NotFound();
        }

        existingProduct.Name = updatedProduct.Name;
        existingProduct.Price = updatedProduct.Price;
        existingProduct.Category = updatedProduct.Category;
        existingProduct.Description = updatedProduct.Description;
        Console.WriteLine(updatedProduct.ImageUrl);
        existingProduct.ImageUrl = updatedProduct.ImageUrl;
        existingProduct.Brand = updatedProduct.Brand;
        existingProduct.Country = updatedProduct.Country;
        existingProduct.Count = updatedProduct.Count;
        existingProduct.Weight = updatedProduct.Weight;

        _context.Products.Update(existingProduct);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/products/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is empty");

        var uploadPath = Path.Combine("wwwroot", "images");

        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        var fileName = Path.GetFileName(file.FileName);
        var filePath = Path.Combine(uploadPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var serverUrl = $"{Request.Scheme}://{Request.Host}"; // Генерация URL сервера
        var imageUrl = $"{serverUrl}/images/{fileName}";
        return Ok(imageUrl);
    }

    [HttpPost("upload-excel")]
    public async Task<IActionResult> UploadExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        try
        {
            using MemoryStream stream = new MemoryStream();
            await file.CopyToAsync(stream);
            using ExcelPackage package = new ExcelPackage(stream);

            ExcelWorksheet? worksheet = package.Workbook.Worksheets[0];
            if (worksheet == null)
            {
                return BadRequest("Invalid Excel file.");
            }

            for (int row = 2; row <= worksheet.Dimension.Rows; row++)
            {
                int id = -1;

                //Id	Name	Description	Price	ImageUrl	Category	Brand	Country	Weight	Count
                id = int.TryParse(worksheet.Cells[row, 1]?.Text, out int parsedID) ? parsedID : -1;
                string? name = worksheet.Cells[row, 2]?.Text?.Trim();
                string? description = worksheet.Cells[row, 3]?.Text?.Trim();
                decimal? price = decimal.TryParse(worksheet.Cells[row, 4]?.Text, out decimal parsedPrice)
                    ? parsedPrice
                    : (decimal?)null;
                //
                string? category = worksheet.Cells[row, 5]?.Text?.Trim();
                string? brand = worksheet.Cells[row, 6]?.Text?.Trim();
                string? country = worksheet.Cells[row, 7]?.Text?.Trim();
                string? weight = worksheet.Cells[row, 8]?.Text?.Trim();
                int? count = int.TryParse(worksheet.Cells[row, 9]?.Text, out int parsedCount)
                    ? parsedCount
                    : (int?)null;
                string? imageUrl = worksheet.Cells[row, 10]?.Text?.Trim();

                if (string.IsNullOrEmpty(name) || price == null || count == null)
                {
                    continue; // Пропускаем некорректные строки
                }

                Product existingProduct = null;
                if (id > 0)
                    // Ищем продукт в базе данных
                    existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

                if (existingProduct != null)
                {
                    // Обновляем существующий продукт
                    existingProduct.Name = name;
                    existingProduct.Category = category;
                    existingProduct.Description = description;
                    existingProduct.Price = price.Value;
                    existingProduct.Count = count.Value;
                    existingProduct.Country = country;
                    existingProduct.Brand = brand;
                    existingProduct.Weight = weight;
                    _context.Products.Update(existingProduct);
                }
                else
                {
                    // Добавляем новый продукт
                    Product newProduct = new Product
                    {
                        Name = name,
                        Category = category,
                        Description = description,
                        Price = price.Value,
                        Count = count.Value,
                        Country = country,
                        Brand = brand,
                        Weight = weight,
                    };
                    _context.Products.Add(newProduct);
                }
            }

            await _context.SaveChangesAsync();
            return Ok("Products have been successfully uploaded.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while processing the Excel file: {ex.Message}");
        }
    }

    [HttpGet("download-excel")]
    public IActionResult DownloadExcel()
    {
        try
        {
            IEnumerable<Product> products = _context.Products.ToList();
            // Генерация Excel-файла с использованием EPPlus
            byte[]? fileBytes = null;
            // Генерация Excel-файла с использованием EPPlus
            // using var package = new ExcelPackage();
            // var sheet = package.Workbook.Worksheets.Add("Products");
            //sheet.Cells["A1"].LoadFromCollection(products, true);

            using (var pck = new ExcelPackage())
            {

                //Add a worksheet
                var sheet = pck.Workbook.Worksheets.Add("Products");
                //Load the datatable into the worksheet...
                sheet.Cells["A1"].LoadFromCollection(products, PrintHeaders: true, TableStyles.Medium9);
                fileBytes = pck.GetAsByteArray();
            }
            

            // for (int i = 0; i < products.Count(); i++)
            // {
            //     sheet.Cells[i + 2, 1].Value = products[i].Id;
            //     sheet.Cells[i + 2, 2].Value = products[i].Name;
            //     sheet.Cells[i + 2, 3].Value = products[i].Description;
            //     sheet.Cells[i + 2, 4].Value = products[i].Price;
            //     sheet.Cells[i + 2, 5].Value = products[i].Category;
            //     sheet.Cells[i + 2, 6].Value = products[i].Brand;
            //     sheet.Cells[i + 2, 7].Value = products[i].Country;
            //     sheet.Cells[i + 2, 8].Value = products[i].Weight;
            //     sheet.Cells[i + 2, 9].Value = products[i].Count;
            //     sheet.Cells[i + 2, 10].Value = products[i].ImageUrl;
            // }

            //fileBytes = package.GetAsByteArray();


            var fileName = "Products.xlsx";

            // Возврат файла
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Не удалось создать Excel файл.");
        }
    }
}