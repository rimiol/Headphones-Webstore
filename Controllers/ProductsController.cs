using Headphones_Webstore.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

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

            connectionType = string.IsNullOrEmpty(connectionType) ? null : connectionType;
            wearingStyle = string.IsNullOrEmpty(wearingStyle) ? null : wearingStyle;
            brand = string.IsNullOrEmpty(brand) ? null : brand;
            Console.WriteLine($"Received filters: connectionType={connectionType}, wearingStyle={wearingStyle}");

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
                    WHERE 
                    (@searchTerm IS NULL OR Name LIKE '%' + @searchTerm + '%')
                    AND (@brand IS NULL OR Brand IN (SELECT value FROM STRING_SPLIT(@brand, ',')))
                    AND (@connectionType IS NULL OR ConnectionType IN (SELECT value FROM STRING_SPLIT(@connectionType, ',')))
                    AND (@wearingStyle IS NULL OR WearingStyle IN (SELECT value FROM STRING_SPLIT(@wearingStyle, ',')))
                    AND (@minPrice IS NULL OR Price >= @minPrice)
                    AND (@maxPrice IS NULL OR Price <= @maxPrice)
                    ORDER BY ProductID
                    OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@brand", string.IsNullOrEmpty(brand) ? DBNull.Value : (object)brand);
                        cmd.Parameters.AddWithValue("@connectionType", string.IsNullOrEmpty(connectionType) ? DBNull.Value : (object)connectionType);
                        cmd.Parameters.AddWithValue("@wearingStyle", string.IsNullOrEmpty(wearingStyle) ? DBNull.Value : (object)wearingStyle);
                        cmd.Parameters.AddWithValue("@minPrice", minPrice ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@maxPrice", maxPrice ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@offset", offset);
                        cmd.Parameters.AddWithValue("@pageSize", pageSize);
                        cmd.Parameters.AddWithValue("@searchTerm", string.IsNullOrEmpty(searchTerm) ? DBNull.Value : (object)searchTerm);

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
                    WHERE 
                    (@searchTerm IS NULL OR Name LIKE '%' + @searchTerm + '%')
                    AND (@brand IS NULL OR Brand IN (SELECT value FROM STRING_SPLIT(@brand, ',')))
                    AND (@connectionType IS NULL OR ConnectionType IN (SELECT value FROM STRING_SPLIT(@connectionType, ',')))
                    AND (@wearingStyle IS NULL OR WearingStyle IN (SELECT value FROM STRING_SPLIT(@wearingStyle, ',')))
                    AND (@minPrice IS NULL OR Price >= @minPrice)
                    AND (@maxPrice IS NULL OR Price <= @maxPrice)";

                    int totalProducts = 0;
                    using (SqlCommand countCmd = new SqlCommand(countQuery, conn))
                    {
                        countCmd.Parameters.AddWithValue("@brand", string.IsNullOrEmpty(brand) ? DBNull.Value : (object)brand);
                        countCmd.Parameters.AddWithValue("@connectionType", string.IsNullOrEmpty(connectionType) ? DBNull.Value : (object)connectionType);
                        countCmd.Parameters.AddWithValue("@wearingStyle", string.IsNullOrEmpty(wearingStyle) ? DBNull.Value : (object)wearingStyle);
                        countCmd.Parameters.AddWithValue("@minPrice", minPrice ?? (object)DBNull.Value);
                        countCmd.Parameters.AddWithValue("@maxPrice", maxPrice ?? (object)DBNull.Value);
                        countCmd.Parameters.AddWithValue("@searchTerm", string.IsNullOrEmpty(searchTerm) ? DBNull.Value : (object)searchTerm);

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
                return StatusCode(500, new
                {
                    error = "Внутренняя ошибка сервера",
                    details = ex.Message
                });
            }
        }
    }
}