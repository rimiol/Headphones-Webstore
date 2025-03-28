using Headphones_Webstore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Headphones_Webstore.Data;

namespace Headphones_Webstore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddItemController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AddItemController(
            ApplicationDbContext context,
            IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] ProductDto productDto)
        {
            // Проверка уникальности названия
            if (await _context.Products.AnyAsync(p => p.Name == productDto.Name))
            {
                return BadRequest(new
                {
                    type = "name",
                    message = "Товар с таким названием уже существует"
                });
            }

            // Нормализация пути к изображению
            var imageUrl = productDto.ImageURL.TrimStart('/');
            var imagePath = Path.Combine(_env.WebRootPath, imageUrl);

            if (!System.IO.File.Exists(imagePath))
            {
                return BadRequest(new
                {
                    type = "imageURL",
                    message = $"Файл '{imageUrl}' не найден в папке wwwroot. Путь: {imagePath}"
                });
            }

            // Создание и сохранение товара
            var product = new Products
            {
                Name = productDto.Name,
                Description = productDto.Description,
                ImageURL = imageUrl,
                Price = productDto.Price,
                ConnectionType = productDto.ConnectionType,
                WearingStyle = productDto.WearingStyle,
                Brand = productDto.Brand
            };

            try
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Товар успешно добавлен!", productId = product.ProductId });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, "Ошибка базы данных: " + ex.Message);
            }
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