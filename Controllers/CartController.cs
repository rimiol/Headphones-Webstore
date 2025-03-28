using Headphones_Webstore.Data;
using Headphones_Webstore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Headphones_Webstore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] CartItemRequest request)
        {
            try
            {
                var session = await GetOrCreateSession();
                if (session == null) return BadRequest("Ошибка сессии");

                // Проверка существования товара
                var productExists = await _context.Products
                    .AnyAsync(p => p.ProductId == request.ProductId);

                if (!productExists) return NotFound("Товар не найден");

                // Поиск существующей записи
                var cartItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci =>
                        ci.SessionID == session.SessionID &&
                        ci.ProductID == request.ProductId);

                if (cartItem != null)
                {
                    cartItem.Quantity++;
                }
                else
                {
                    _context.CartItems.Add(new CartItems
                    {
                        SessionID = session.SessionID,
                        ProductID = request.ProductId,
                        Quantity = 1,
                        AddedAt = DateTime.UtcNow
                    });
                }

                await _context.SaveChangesAsync();

                // Получение общего количества
                var totalItems = await _context.CartItems
                    .Where(ci => ci.SessionID == session.SessionID)
                    .SumAsync(ci => ci.Quantity);

                return Ok(new { totalItems });
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

        [HttpGet("count")]
        public async Task<IActionResult> GetCartItemCount()
        {
            var session = await GetOrCreateSession();
            if (session == null) return Ok(new { totalItems = 0 });

            var totalItems = await _context.CartItems
                .Where(ci => ci.SessionID == session.SessionID)
                .SumAsync(ci => ci.Quantity);

            return Ok(new { totalItems = totalItems });
        }

        private async Task<Sessions?> GetOrCreateSession()
        {
            var sessionId = Request.Cookies["SessionID"];

            // Существующая сессия
            if (Guid.TryParse(sessionId, out var sessionGuid))
            {
                var existingSession = await _context.Sessions
                    .FirstOrDefaultAsync(s => s.SessionID == sessionGuid);

                if (existingSession != null) return existingSession;
            }

            // Создание новой сессии
            var newSession = new Sessions
            {
                SessionID = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Sessions.Add(newSession);
            await _context.SaveChangesAsync();

            Response.Cookies.Append("SessionID", newSession.SessionID.ToString(), new CookieOptions
            {
                Expires = DateTime.UtcNow.AddDays(7),
                HttpOnly = true,
                IsEssential = true,
                Secure = true,
                SameSite = SameSiteMode.Lax
            });

            return newSession;
        }
    }

    public class CartItemRequest
    {
        public int ProductId { get; set; }
    }
}