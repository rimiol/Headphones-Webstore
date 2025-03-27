using Microsoft.EntityFrameworkCore;
using Headphones_Webstore.Models;

namespace Headphones_Webstore.Data

{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) 
        { 
                   
        }

        public DbSet<Products> Products { get; set; } 
    
        public DbSet<Sessions> Sessions { get; set; }

        public DbSet<CartItems> CartItems { get; set; }
    }
}
