using Microsoft.AspNetCore.Mvc;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Headphones_Webstore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CartController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Добавление товара в корзину
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] CartItemRequest request)
        {
            var sessionId = await GetOrCreateSessionId();
            if (string.IsNullOrEmpty(sessionId)) return BadRequest("Ошибка сессии");

            try
            {
                using (var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await conn.OpenAsync();

                    // Проверяем существование товара
                    var productExists = await CheckProductExists(conn, request.ProductId);
                    if (!productExists) return NotFound("Товар не найден");

                    // Обновляем или добавляем товар в корзину
                    var query = @"
                        MERGE INTO CartItems AS target
                        USING (VALUES (@SessionID, @ProductID, 1)) AS source (SessionID, ProductID, Quantity)
                        ON target.SessionID = source.SessionID AND target.ProductID = source.ProductID
                        WHEN MATCHED THEN
                            UPDATE SET Quantity = target.Quantity + 1
                        WHEN NOT MATCHED THEN
                            INSERT (SessionID, ProductID, Quantity, AddedAt)
                            VALUES (source.SessionID, source.ProductID, source.Quantity, GETDATE());
                        
                        SELECT SUM(Quantity) FROM CartItems WHERE SessionID = @SessionID;";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SessionID", Guid.Parse(sessionId));
                        cmd.Parameters.AddWithValue("@ProductID", request.ProductId);

                        var totalItems = (int)await cmd.ExecuteScalarAsync();
                        return Ok(new { totalItems });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Получение количества товаров в корзине
        [HttpGet("count")]
        public async Task<IActionResult> GetCartItemCount()
        {
            var sessionId = await GetOrCreateSessionId();
            if (string.IsNullOrEmpty(sessionId)) return Ok(new { totalItems = 0 });

            try
            {
                using (var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await conn.OpenAsync();

                    var query = "SELECT SUM(Quantity) FROM CartItems WHERE SessionID = @SessionID";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SessionID", Guid.Parse(sessionId));
                        var result = await cmd.ExecuteScalarAsync();

                        return Ok(new { totalItems = result is DBNull ? 0 : (int)result });
                    }
                }
            }
            catch
            {
                return Ok(new { totalItems = 0 });
            }
        }

        private async Task<string> GetOrCreateSessionId()
        {
            var sessionCookie = Request.Cookies["SessionID"];
            if (Guid.TryParse(sessionCookie, out var sessionId))
            {
                return sessionCookie;
            }

            // Создаем новую сессию
            var newSessionId = Guid.NewGuid();
            Response.Cookies.Append("SessionID", newSessionId.ToString(), new CookieOptions
            {
                Expires = DateTime.Now.AddDays(7),
                HttpOnly = true,
                IsEssential = true
            });

            using (var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await conn.OpenAsync();
                var query = "INSERT INTO Sessions (SessionID, CreatedAt) VALUES (@SessionID, GETDATE())";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SessionID", newSessionId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return newSessionId.ToString();
        }

        private async Task<bool> CheckProductExists(SqlConnection conn, int productId)
        {
            var query = "SELECT 1 FROM Products WHERE ProductId = @ProductId";
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ProductId", productId);
                return await cmd.ExecuteScalarAsync() != null;
            }
        }
    }

    public class CartItemRequest
    {
        public int ProductId { get; set; }
    }
}