using Headphones_Webstore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Headphones_Webstore.Data;
using Microsoft.Extensions.Logging;

namespace Headphones_Webstore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddItemController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<AddItemController> _logger;

        public AddItemController(
            ApplicationDbContext context,
            IWebHostEnvironment env,
            ILogger<AddItemController> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] ProductDto productDto)
        {
            try
            {
                // Валидация модели
                var imageUrl = productDto.ImageURL?.Trim();
                if (string.IsNullOrWhiteSpace(imageUrl))
                {
                    return BadRequest(new
                    {
                        type = "imageURL",
                        message = "URL изображения обязателен"
                    });
                }

                imageUrl = imageUrl.StartsWith("~/") ? imageUrl[2..] : imageUrl;
                var webRootPath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                var fullPath = Path.Combine(webRootPath, imageUrl.Replace('/', Path.DirectorySeparatorChar));

                Console.WriteLine($"WebRootPath: {webRootPath}");
                Console.WriteLine($"Requested image path: {fullPath}");

                if (!System.IO.File.Exists(fullPath))
                {
                    return NotFound(new
                    {
                        type = "imageURL",
                        message = "Файл изображения не найден",
                        details = new
                        {
                            requestedPath = imageUrl,
                            fullPhysicalPath = fullPath,
                            webRootExists = Directory.Exists(webRootPath)
                        }
                    });
                }

                // Проверка уникальности названия
                var normalizedName = productDto.Name.Trim().ToLower();
                bool nameExists = await _context.Products
                    .AnyAsync(p => p.Name.ToLower() == normalizedName);

                if (nameExists)
                {
                    return Conflict(new
                    {
                        type = "name",
                        message = "Товар с таким названием уже существует"
                    });
                }

                // Создание объекта продукта
                var product = new Products
                {
                    Name = productDto.Name.Trim(),
                    Description = productDto.Description.Trim(),
                    ImageURL = imageUrl,
                    Price = productDto.Price,
                    ConnectionType = productDto.ConnectionType.Trim(),
                    WearingStyle = productDto.WearingStyle.Trim(),
                    Brand = productDto.Brand.Trim()
                };

                // Сохранение в базе данных
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProduct), new { id = product.ProductId }, new
                {
                    message = "Товар успешно добавлен",
                    productId = product.ProductId,
                    details = new
                    {
                        product.Name,
                        product.Price,
                        product.ImageURL
                    }
                });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error while adding product");
                return StatusCode(500, new
                {
                    type = "database",
                    message = "Ошибка базы данных",
                    details = dbEx.InnerException?.Message ?? dbEx.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Server error while adding product");
                return StatusCode(500, new
                {
                    type = "server",
                    message = "Внутренняя ошибка сервера",
                    details = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductId == id);

            return product == null
                ? NotFound(new { message = "Товар не найден" })
                : Ok(product);
        }
    }

    public class ProductDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageURL { get; set; }
        public decimal Price { get; set; }
        public string ConnectionType { get; set; }
        public string WearingStyle { get; set; }
        public string Brand { get; set; }
    }
}