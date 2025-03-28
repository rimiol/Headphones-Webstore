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
            [FromQuery] string? brand = null,
            [FromQuery] string? connectionType = null,
            [FromQuery] string? wearingStyle = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string? searchTerm = null)
        {
            if (page < 1)
                return BadRequest("Номер страницы должен быть больше 0");

            int pageSize = 8;
            var query = _context.Products.AsQueryable();

            // Фильтрация
            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(p => p.Name.Contains(searchTerm));

            if (!string.IsNullOrEmpty(brand))
            {
                var brands = brand.Split(',', StringSplitOptions.RemoveEmptyEntries);
                query = query.Where(p => brands.Contains(p.Brand));
            }

            if (!string.IsNullOrEmpty(connectionType))
            {
                var connectionTypes = connectionType.Split(',', StringSplitOptions.RemoveEmptyEntries);
                query = query.Where(p => connectionTypes.Contains(p.ConnectionType));
            }

            if (!string.IsNullOrEmpty(wearingStyle))
            {
                var styles = wearingStyle.Split(',', StringSplitOptions.RemoveEmptyEntries);
                query = query.Where(p => styles.Contains(p.WearingStyle));
            }

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice);

            // Пагинация
            int totalProducts = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

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
    }
}