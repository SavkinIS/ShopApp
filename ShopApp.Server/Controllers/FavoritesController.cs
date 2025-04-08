using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopApp.Models;
using ShopApp.Server.Data;
using System.Security.Claims;

namespace ShopApp.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FavoritesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FavoritesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Получить список избранных товаров
        [HttpGet]
        public async Task<IActionResult> GetFavorites()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var favorites = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Product)
                .Select(f => f.Product)
                .ToListAsync();
            return Ok(favorites);
        }

        // Добавить товар в избранное
        [HttpPost("add")]
        public async Task<IActionResult> AddToFavorites([FromBody] int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (_context.Favorites.Any(f => f.UserId == userId && f.ProductId == productId))
            {
                return BadRequest("Product is already in favorites.");
            }

            var favorite = new Favorite
            {
                UserId = userId,
                ProductId = productId
            };
            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // Удалить товар из избранного
        [HttpDelete("remove/{productId}")]
        public async Task<IActionResult> RemoveFromFavorites(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);
            if (favorite == null)
            {
                return NotFound("Product not found in favorites.");
            }

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();
            return Ok();
        }
        
        // Проверка, является ли товар избранным
        [HttpGet("isFavorite/{productId}")]
        public async Task<IActionResult> IsFavorite(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isFavorite = await _context.Favorites
                .AnyAsync(f => f.UserId == userId && f.ProductId == productId);
            return Ok(isFavorite);
        }
    }
}