using Headphones_Webstore.Models;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Headphones_Webstore.Models;

namespace Headphones_Webstore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ProductsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET: api/products?page=1&brand=Sony&connectionType=Bluetooth&minPrice=1000&maxPrice=5000
        [HttpGet]
        public async Task<IActionResult> GetProducts(
            [FromQuery] int page = 1,
            [FromQuery] string? brand = null,
            [FromQuery] string? connectionType = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string? searchTerm = null)
        {
            if (page < 1)
            {
                return BadRequest("Номер страницы должен быть больше 0");
            }

            int pageSize = 8;
            int offset = (page - 1) * pageSize;
            List<Products> products = new List<Products>();

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    string query = @"
                    SELECT ProductID, Name, Description, ImageURL, Price, ConnectionType, WearingStyle, Brand
                    FROM Products
                    WHERE (@searchTerm IS NULL OR Name LIKE '%' + @searchTerm + '%')
                    ORDER BY ProductID
                    OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@brand", (object)brand ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@connectionType", (object)connectionType ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@minPrice", (object)minPrice ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@maxPrice", (object)maxPrice ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@offset", offset);
                        cmd.Parameters.AddWithValue("@pageSize", pageSize);
                        cmd.Parameters.AddWithValue("@searchTerm", string.IsNullOrEmpty(searchTerm) ? DBNull.Value : searchTerm);

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Products prod = new Products
                                {
                                    ProductId = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    Description = reader.GetString(2),
                                    ImageURL = reader.GetString(3),
                                    Price = reader.GetDecimal(4),
                                    ConnectionType = reader.GetString(5),
                                    WearingStyle = reader.GetString(6),
                                    Brand = reader.GetString(7)
                                };
                                products.Add(prod);
                            }
                        }
                    }

                    // Получение общего числа товаров для расчёта количества страниц
                    string countQuery = @"
                    SELECT COUNT(*) 
                    FROM Products
                    WHERE (@searchTerm IS NULL OR Name LIKE '%' + @searchTerm + '%')";

                    int totalProducts = 0;
                    using (SqlCommand countCmd = new SqlCommand(countQuery, conn))
                    {
                        countCmd.Parameters.AddWithValue("@brand", (object)brand ?? DBNull.Value);
                        countCmd.Parameters.AddWithValue("@connectionType", (object)connectionType ?? DBNull.Value);
                        countCmd.Parameters.AddWithValue("@minPrice", (object)minPrice ?? DBNull.Value);
                        countCmd.Parameters.AddWithValue("@maxPrice", (object)maxPrice ?? DBNull.Value);
                        countCmd.Parameters.AddWithValue("@searchTerm", string.IsNullOrEmpty(searchTerm) ? DBNull.Value : searchTerm);
                        totalProducts = (int)await countCmd.ExecuteScalarAsync();
                    }

                    int totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

                    var result = new
                    {
                        TotalProducts = totalProducts,
                        Page = page,
                        PageSize = pageSize,
                        TotalPages = totalPages,
                        Products = products
                    };

                    return Ok(result);

                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Внутренняя ошибка сервера: " + ex.Message);
            }
        }
    }
}