using Headphones_Webstore.Data;
using Headphones_Webstore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace Headphones_Webstore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? connectionType = null,
        [FromQuery] string? wearingStyle = null,
        [FromQuery] string? brand = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null)
        {
            try
            {
                if (page < 1)
                    return BadRequest("Номер страницы должен быть больше 0");

                if (minPrice < 0 || maxPrice < 0)
                    return BadRequest("Цена не может быть отрицательной");

                int pageSize = 8;
                var query = _context.Products.AsQueryable();

                // Фильтрация
                if (!string.IsNullOrEmpty(searchTerm))
                    query = query.Where(p => p.Name.Contains(searchTerm));

                if (!string.IsNullOrEmpty(brand))
                {
                    var brands = brand.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (brands.Any())
                        query = query.Where(p => brands.Contains(p.Brand));
                }

                if (!string.IsNullOrEmpty(connectionType))
                {
                    var types = connectionType.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (types.Any())
                        query = query.Where(p => types.Contains(p.ConnectionType));
                }

                if (!string.IsNullOrEmpty(wearingStyle))
                {
                    var styles = wearingStyle.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (styles.Any())
                        query = query.Where(p => styles.Contains(p.WearingStyle));
                }

                if (minPrice.HasValue)
                    query = query.Where(p => p.Price >= minPrice);

                if (maxPrice.HasValue)
                    query = query.Where(p => p.Price <= maxPrice);

                // Пагинация
                int totalProducts = await query.CountAsync();
                int totalPages = totalProducts > 0 ? (int)Math.Ceiling(totalProducts / (double)pageSize) : 0;

                var products = await query
                    .OrderBy(p => p.ProductId)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(new
                {
                    TotalProducts = totalProducts,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    Products = products
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Внутренняя ошибка сервера",
                    details = ex.Message
                });
            }
        }

        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSearchSuggestions([FromQuery] string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Ok(Array.Empty<object>());

            var suggestions = await _context.Products
                .Where(p => p.Name.Contains(searchTerm))
                .Select(p => new {
                    Id = p.ProductId,
                    Name = p.Name
                })
                .Take(5)
                .ToListAsync();

            return Ok(suggestions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            try
            {
                var product = await _context.Products
                    .AsNoTracking()
                    .Select(p => new
                    {
                        ProductId = p.ProductId,
                        Name = p.Name,
                        Price = p.Price,
                        ImageURL = p.ImageURL,
                        Description = p.Description,
                        ConnectionType = p.ConnectionType,
                        WearingStyle = p.WearingStyle,
                        Brand = p.Brand
                    })
                    .FirstOrDefaultAsync(p => p.ProductId == id);

                if (product == null)
                {
                    return NotFound(new { error = "Товар не найден" });
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }
    }
}