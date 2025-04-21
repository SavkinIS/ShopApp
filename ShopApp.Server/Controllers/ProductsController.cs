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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        var products = await _context.Products.ToListAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProductById(int id)
    {
        var product = await _context.Products
            .Include(p => p is Tool ? ((Tool)p).Id : 0)
            .Include(p => p is Accessory ? ((Accessory)p).Id : 0)
            .Include(p => p is Clothing ? ((Clothing)p).Id : 0)
            .Include(p => p is MasterClass ? ((MasterClass)p).Id : 0)
            .Include(p => p is Yarn ? ((Yarn)p).Id : 0)
            .Include(p => p is YarnBobbin ? ((YarnBobbin)p).Id : 0)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpPost]
public async Task<ActionResult<Product>> AddProduct([FromBody] Product newProduct)
{
    if (newProduct == null)
    {
        return BadRequest("Product data is null.");
    }

    if (string.IsNullOrWhiteSpace(newProduct.Name))
    {
        return BadRequest("Product name cannot be empty.");
    }

    try
    {
        // Получаем тип объекта
        string type = newProduct.GetType().Name;
        Console.WriteLine($"Received type: {type}");

        Product createdProduct;

        switch (type)
        {
            case "Tool":
                var tool = new Tool
                {
                    Name = newProduct.Name,
                    Description = newProduct.Description,
                    Price = newProduct.Price,
                    Brand = newProduct.Brand,
                    Country = newProduct.Country,
                    Count = newProduct.Count,
                    ImageUrl = newProduct.ImageUrl,
                    ImageUrl2 = newProduct.ImageUrl2,
                    ImageUrl3 = newProduct.ImageUrl3,
                    ImageUrl4 = newProduct.ImageUrl4,
                    Material = newProduct is Tool t ? t.Material : string.Empty,
                    Size = newProduct is Tool t2 ? t2.Size : string.Empty,
                    Purpose = newProduct is Tool t3 ? t3.Purpose : string.Empty,
                    WeightGramm = newProduct is Tool t4 ? t4.WeightGramm : 0
                };
                _context.Tools.Add(tool);
                createdProduct = tool;
                break;

            case "Accessory":
                var accessory = new Accessory
                {
                    Name = newProduct.Name,
                    Description = newProduct.Description,
                    Price = newProduct.Price,
                    Brand = newProduct.Brand,
                    Country = newProduct.Country,
                    Count = newProduct.Count,
                    ImageUrl = newProduct.ImageUrl,
                    ImageUrl2 = newProduct.ImageUrl2,
                    ImageUrl3 = newProduct.ImageUrl3,
                    ImageUrl4 = newProduct.ImageUrl4,
                    Material = newProduct is Accessory a ? a.Material : string.Empty,
                    Size = newProduct is Accessory a2 ? a2.Size : string.Empty,
                    Type = newProduct is Accessory a3 ? a3.Type : string.Empty,
                    WeightGramm = newProduct is Accessory a4 ? a4.WeightGramm : 0
                };
                _context.Accessories.Add(accessory);
                createdProduct = accessory;
                break;

            case "Clothing":
                var clothing = new Clothing
                {
                    Name = newProduct.Name,
                    Description = newProduct.Description,
                    Price = newProduct.Price,
                    Brand = newProduct.Brand,
                    Country = newProduct.Country,
                    Count = newProduct.Count,
                    ImageUrl = newProduct.ImageUrl,
                    ImageUrl2 = newProduct.ImageUrl2,
                    ImageUrl3 = newProduct.ImageUrl3,
                    ImageUrl4 = newProduct.ImageUrl4,
                    Size = newProduct is Clothing c ? c.Size : string.Empty,
                    Fabric = newProduct is Clothing c2 ? c2.Fabric : string.Empty,
                    Season = newProduct is Clothing c3 ? c3.Season : string.Empty,
                    Color = newProduct is Clothing c4 ? c4.Color : string.Empty,
                    WeightGramm = newProduct is Clothing c5 ? c5.WeightGramm : 0
                };
                _context.Clothing.Add(clothing);
                createdProduct = clothing;
                break;

            case "MasterClass":
                var masterClass = new MasterClass
                {
                    Name = newProduct.Name,
                    Description = newProduct.Description,
                    Price = newProduct.Price,
                    Brand = newProduct.Brand,
                    Country = newProduct.Country,
                    Count = newProduct.Count,
                    ImageUrl = newProduct.ImageUrl,
                    ImageUrl2 = newProduct.ImageUrl2,
                    ImageUrl3 = newProduct.ImageUrl3,
                    ImageUrl4 = newProduct.ImageUrl4,
                    EventDate = newProduct is MasterClass mc ? mc.EventDate : DateTime.Now,
                    DurationHours = newProduct is MasterClass mc2 ? mc2.DurationHours : 0,
                    DifficultyLevel = newProduct is MasterClass mc3 ? mc3.DifficultyLevel : string.Empty,
                    Format = newProduct is MasterClass mc4 ? mc4.Format : string.Empty
                };
                _context.MasterClasses.Add(masterClass);
                createdProduct = masterClass;
                break;

            case "Yarn":
                var yarn = new Yarn
                {
                    Name = newProduct.Name,
                    Description = newProduct.Description,
                    Price = newProduct.Price,
                    Brand = newProduct.Brand,
                    Country = newProduct.Country,
                    Count = newProduct.Count,
                    ImageUrl = newProduct.ImageUrl,
                    ImageUrl2 = newProduct.ImageUrl2,
                    ImageUrl3 = newProduct.ImageUrl3,
                    ImageUrl4 = newProduct.ImageUrl4,
                    Type = newProduct is Yarn y ? y.Type : string.Empty,
                    Color = newProduct is Yarn y2 ? y2.Color : string.Empty,
                    WeightGramm = newProduct is Yarn y3 ? y3.WeightGramm : 0,
                    ToolsSize = newProduct is Yarn y4 ? y4.ToolsSize : string.Empty,
                    Length = newProduct is Yarn y5 ? y5.Length : 0
                };
                _context.Yarns.Add(yarn);
                createdProduct = yarn;
                break;

            case "YarnBobbin":
                var yarnBobbin = new YarnBobbin
                {
                    Name = newProduct.Name,
                    Description = newProduct.Description,
                    Price = newProduct.Price,
                    Brand = newProduct.Brand,
                    Country = newProduct.Country,
                    Count = newProduct.Count,
                    ImageUrl = newProduct.ImageUrl,
                    ImageUrl2 = newProduct.ImageUrl2,
                    ImageUrl3 = newProduct.ImageUrl3,
                    ImageUrl4 = newProduct.ImageUrl4,
                    Type = newProduct is YarnBobbin yb ? yb.Type : string.Empty,
                    Color = newProduct is YarnBobbin yb2 ? yb2.Color : string.Empty,
                    WeightGramm = newProduct is YarnBobbin yb3 ? yb3.WeightGramm : 0,
                    ToolsSize = newProduct is YarnBobbin yb4 ? yb4.ToolsSize : string.Empty,
                    Length = newProduct is YarnBobbin yb5 ? yb5.Length : 0
                };
                _context.YarnBobbins.Add(yarnBobbin);
                createdProduct = yarnBobbin;
                break;

            default:
                return BadRequest($"Product type is not supported. Received type: {type}");
        }

        await _context.SaveChangesAsync();

        // Логируем тип после сохранения
        Console.WriteLine($"Type after SaveChanges: {createdProduct.GetType().FullName}");

        return CreatedAtAction(nameof(GetProductById), new { id = createdProduct.Id }, createdProduct);
    }
    catch (DbUpdateException ex)
    {
        return StatusCode(500, $"Database error: {ex.InnerException?.Message ?? ex.Message}");
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"An error occurred: {ex.Message}");
    }
}

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateProduct(int id, [FromBody] Product updatedProduct)
    {
        if (updatedProduct == null || id != updatedProduct.Id)
        {
            return BadRequest("Product ID mismatch or invalid product data.");
        }

        var existingProduct = await _context.Products
            .Include(p => p is Tool ? ((Tool)p).Id : 0)
            .Include(p => p is Accessory ? ((Accessory)p).Id : 0)
            .Include(p => p is Clothing ? ((Clothing)p).Id : 0)
            .Include(p => p is MasterClass ? ((MasterClass)p).Id : 0)
            .Include(p => p is Yarn ? ((Yarn)p).Id : 0)
            .Include(p => p is YarnBobbin ? ((YarnBobbin)p).Id : 0)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (existingProduct == null)
        {
            return NotFound();
        }

        existingProduct.Name = updatedProduct.Name;
        existingProduct.Description = updatedProduct.Description;
        existingProduct.Price = updatedProduct.Price;
        existingProduct.Brand = updatedProduct.Brand;
        existingProduct.Country = updatedProduct.Country;
        existingProduct.Count = updatedProduct.Count;
        existingProduct.ImageUrl = updatedProduct.ImageUrl;
        existingProduct.ImageUrl2 = updatedProduct.ImageUrl2;
        existingProduct.ImageUrl3 = updatedProduct.ImageUrl3;
        existingProduct.ImageUrl4 = updatedProduct.ImageUrl4;

        if (existingProduct is Tool existingTool && updatedProduct is Tool updatedTool)
        {
            existingTool.Material = updatedTool.Material;
            existingTool.Size = updatedTool.Size;
            existingTool.Purpose = updatedTool.Purpose;
            existingTool.WeightGramm = updatedTool.WeightGramm;
        }
        else if (existingProduct is Accessory existingAccessory && updatedProduct is Accessory updatedAccessory)
        {
            existingAccessory.Material = updatedAccessory.Material;
            existingAccessory.Size = updatedAccessory.Size;
            existingAccessory.Type = updatedAccessory.Type;
            existingAccessory.WeightGramm = updatedAccessory.WeightGramm;
        }
        else if (existingProduct is Clothing existingClothing && updatedProduct is Clothing updatedClothing)
        {
            existingClothing.Size = updatedClothing.Size;
            existingClothing.Fabric = updatedClothing.Fabric;
            existingClothing.Season = updatedClothing.Season;
            existingClothing.Color = updatedClothing.Color;
            existingClothing.WeightGramm = updatedClothing.WeightGramm;
        }
        else if (existingProduct is MasterClass existingMasterClass && updatedProduct is MasterClass updatedMasterClass)
        {
            existingMasterClass.EventDate = updatedMasterClass.EventDate;
            existingMasterClass.DurationHours = updatedMasterClass.DurationHours;
            existingMasterClass.DifficultyLevel = updatedMasterClass.DifficultyLevel;
            existingMasterClass.Format = updatedMasterClass.Format;
        }
        else if (existingProduct is YarnBobbin existingYarnBobbin && updatedProduct is YarnBobbin updatedYarnBobbin)
        {
            ((Yarn)existingYarnBobbin).Type = updatedYarnBobbin.Type;
            ((Yarn)existingYarnBobbin).Color = updatedYarnBobbin.Color;
            ((Yarn)existingYarnBobbin).WeightGramm = updatedYarnBobbin.WeightGramm;
            ((Yarn)existingYarnBobbin).ToolsSize = updatedYarnBobbin.ToolsSize;
            ((Yarn)existingYarnBobbin).Length = updatedYarnBobbin.Length;
        }
        else if (existingProduct is Yarn existingYarn && updatedProduct is Yarn updatedYarn)
        {
            existingYarn.Type = updatedYarn.Type;
            existingYarn.Color = updatedYarn.Color;
            existingYarn.WeightGramm = updatedYarn.WeightGramm;
            existingYarn.ToolsSize = updatedYarn.ToolsSize;
            existingYarn.Length = updatedYarn.Length;
        }
        else
        {
            return BadRequest("Product type mismatch or unsupported type.");
        }

        _context.Products.Update(existingProduct);
        await _context.SaveChangesAsync();
        return NoContent();
    }

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

        var serverUrl = $"{Request.Scheme}://{Request.Host}";
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
                int id = int.TryParse(worksheet.Cells[row, 1]?.Text, out int parsedID) ? parsedID : -1;
                string? name = worksheet.Cells[row, 2]?.Text?.Trim();
                string? description = worksheet.Cells[row, 3]?.Text?.Trim();
                decimal? price = decimal.TryParse(worksheet.Cells[row, 4]?.Text, out decimal parsedPrice)
                    ? parsedPrice
                    : (decimal?)null;
                string? type = worksheet.Cells[row, 5]?.Text?.Trim();
                string? brand = worksheet.Cells[row, 6]?.Text?.Trim();
                string? country = worksheet.Cells[row, 7]?.Text?.Trim();
                string? weight = worksheet.Cells[row, 8]?.Text?.Trim();
                int? count = int.TryParse(worksheet.Cells[row, 9]?.Text, out int parsedCount)
                    ? parsedCount
                    : (int?)null;
                string? imageUrl = worksheet.Cells[row, 10]?.Text?.Trim();
                string? color = worksheet.Cells[row, 11]?.Text?.Trim();

                if (string.IsNullOrEmpty(name) || price == null || count == null || string.IsNullOrEmpty(type))
                {
                    continue;
                }

                Product newProduct = type.ToLower() switch
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
                    "yarnbobbin" => new YarnBobbin
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

                if (newProduct == null)
                {
                    continue;
                }

                newProduct.Name = name;
                newProduct.Description = description;
                newProduct.Price = price.Value;
                newProduct.Brand = brand;
                newProduct.Country = country;
                newProduct.Count = count.Value;
                newProduct.ImageUrl = imageUrl;

                Product existingProduct = null;
                if (id > 0)
                {
                    existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
                }

                if (existingProduct != null)
                {
                    existingProduct.Name = newProduct.Name;
                    existingProduct.Description = newProduct.Description;
                    existingProduct.Price = newProduct.Price;
                    existingProduct.Brand = newProduct.Brand;
                    existingProduct.Country = newProduct.Country;
                    existingProduct.Count = newProduct.Count;
                    existingProduct.ImageUrl = newProduct.ImageUrl;

                    if (existingProduct is Tool existingTool && newProduct is Tool updatedTool)
                    {
                        existingTool.Material = updatedTool.Material;
                        existingTool.Size = updatedTool.Size;
                        existingTool.Purpose = updatedTool.Purpose;
                        existingTool.WeightGramm = updatedTool.WeightGramm;
                    }
                    else if (existingProduct is Accessory existingAccessory && newProduct is Accessory updatedAccessory)
                    {
                        existingAccessory.Material = updatedAccessory.Material;
                        existingAccessory.Size = updatedAccessory.Size;
                        existingAccessory.Type = updatedAccessory.Type;
                        existingAccessory.WeightGramm = updatedAccessory.WeightGramm;
                    }
                    else if (existingProduct is Clothing existingClothing && newProduct is Clothing updatedClothing)
                    {
                        existingClothing.Size = updatedClothing.Size;
                        existingClothing.Fabric = updatedClothing.Fabric;
                        existingClothing.Season = updatedClothing.Season;
                        existingClothing.Color = updatedClothing.Color;
                        existingClothing.WeightGramm = updatedClothing.WeightGramm;
                    }
                    else if (existingProduct is MasterClass existingMasterClass &&
                             newProduct is MasterClass updatedMasterClass)
                    {
                        existingMasterClass.EventDate = updatedMasterClass.EventDate;
                        existingMasterClass.DurationHours = updatedMasterClass.DurationHours;
                        existingMasterClass.DifficultyLevel = updatedMasterClass.DifficultyLevel;
                        existingMasterClass.Format = updatedMasterClass.Format;
                    }
                    else if (existingProduct is YarnBobbin existingYarnBobbin &&
                             newProduct is YarnBobbin updatedYarnBobbin)
                    {
                        ((Yarn)existingYarnBobbin).Type = updatedYarnBobbin.Type;
                        ((Yarn)existingYarnBobbin).Color = updatedYarnBobbin.Color;
                        ((Yarn)existingYarnBobbin).WeightGramm = updatedYarnBobbin.WeightGramm;
                        ((Yarn)existingYarnBobbin).ToolsSize = updatedYarnBobbin.ToolsSize;
                        ((Yarn)existingYarnBobbin).Length = updatedYarnBobbin.Length;
                    }
                    else if (existingProduct is Yarn existingYarn && newProduct is Yarn updatedYarn)
                    {
                        existingYarn.Type = updatedYarn.Type;
                        existingYarn.Color = updatedYarn.Color;
                        existingYarn.WeightGramm = updatedYarn.WeightGramm;
                        existingYarn.ToolsSize = updatedYarn.ToolsSize;
                        existingYarn.Length = updatedYarn.Length;
                    }
                    else
                    {
                        continue;
                    }

                    _context.Products.Update(existingProduct);
                }
                else
                {
                    if (newProduct is Tool tool)
                    {
                        _context.Tools.Add(tool);
                    }
                    else if (newProduct is Accessory accessory)
                    {
                        _context.Accessories.Add(accessory);
                    }
                    else if (newProduct is Clothing clothing)
                    {
                        _context.Clothing.Add(clothing);
                    }
                    else if (newProduct is MasterClass masterClass)
                    {
                        _context.MasterClasses.Add(masterClass);
                    }
                    else if (newProduct is YarnBobbin yarnBobbin)
                    {
                        _context.YarnBobbins.Add(yarnBobbin);
                    }
                    else if (newProduct is Yarn yarn)
                    {
                        _context.Yarns.Add(yarn);
                    }
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
            var products = _context.Products
                .Include(p => p is Tool ? ((Tool)p).Id : 0)
                .Include(p => p is Accessory ? ((Accessory)p).Id : 0)
                .Include(p => p is Clothing ? ((Clothing)p).Id : 0)
                .Include(p => p is MasterClass ? ((MasterClass)p).Id : 0)
                .Include(p => p is Yarn ? ((Yarn)p).Id : 0)
                .Include(p => p is YarnBobbin ? ((YarnBobbin)p).Id : 0)
                .ToList();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Products");

            worksheet.Cells[1, 1].Value = "Id";
            worksheet.Cells[1, 2].Value = "Name";
            worksheet.Cells[1, 3].Value = "Description";
            worksheet.Cells[1, 4].Value = "Price";
            worksheet.Cells[1, 5].Value = "Type";
            worksheet.Cells[1, 6].Value = "Brand";
            worksheet.Cells[1, 7].Value = "Country";
            worksheet.Cells[1, 8].Value = "WeightGramm";
            worksheet.Cells[1, 9].Value = "Count";
            worksheet.Cells[1, 10].Value = "ImageUrl";
            worksheet.Cells[1, 11].Value = "Color";

            int row = 2;
            foreach (var product in products)
            {
                worksheet.Cells[row, 1].Value = product.Id;
                worksheet.Cells[row, 2].Value = product.Name;
                worksheet.Cells[row, 3].Value = product.Description;
                worksheet.Cells[row, 4].Value = product.Price;
                worksheet.Cells[row, 5].Value = product switch
                {
                    Tool _ => "tool",
                    Accessory _ => "accessory",
                    Clothing _ => "clothing",
                    MasterClass _ => "masterclass",
                    YarnBobbin _ => "yarnbobbin",
                    Yarn _ => "yarn",
                    _ => "unknown"
                };
                worksheet.Cells[row, 6].Value = product.Brand;
                worksheet.Cells[row, 7].Value = product.Country;
                if (product is YarnBobbin yarnBobbins)
                {
                    worksheet.Cells[row, 8].Value = yarnBobbins.WeightGramm;
                }
                else
                {
                    worksheet.Cells[row, 8].Value = product switch
                    {
                        Tool tool => tool.WeightGramm,
                        Accessory accessory => accessory.WeightGramm,
                        Clothing clothing => clothing.WeightGramm,
                        Yarn yarn => yarn.WeightGramm,
                        _ => (float?)null
                    };
                }
                
                worksheet.Cells[row, 9].Value = product.Count;
                worksheet.Cells[row, 10].Value = product.ImageUrl;
                if (product is YarnBobbin yarnBobbin)
                {
                    worksheet.Cells[row, 11].Value = yarnBobbin.Color;
                }
                else
                {
                    worksheet.Cells[row, 11].Value = product switch
                    {
                        Clothing clothing => clothing.Color,
                        Yarn yarn => yarn.Color, 
                        _ => null
                    };
                }
                

                row++;
            }

            var fileBytes = package.GetAsByteArray();
            var fileName = "Products.xlsx";

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Failed to create Excel file: {ex.Message}");
        }
    }
}