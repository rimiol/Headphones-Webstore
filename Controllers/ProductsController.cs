using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Headphones_Webstore.Models;

namespace Headphones_Webstore.Controllers
{
    //localhost:xxxx/api/products
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ProductsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET: api/products?page=1
        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] int page = 1)
        {
            if(page < 1)
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
                        ORDER BY ProductID
                        OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@offset", offset);
                        cmd.Parameters.AddWithValue("@pageSize", pageSize);

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

                    string countQuery = "SELECT COUNT(*) FROM Products";
                    int totalProducts = 0;
                    using (SqlCommand countCmd = new SqlCommand(countQuery, conn))
                    {
                        totalProducts = (int)await countCmd.ExecuteScalarAsync();
                    }

                    int totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

                    var result = new
                    {
                        Products = products,
                        TotalPages = totalPages,
                        CurrentPage = page
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
