using System.ComponentModel.DataAnnotations;

namespace Headphones_Webstore.Models
{
    public class Products
    {
        [Key]
        public required int ProductId { get; set; }

        public required string Name { get; set; }

        public required string Description { get; set; }

        public required string ImageURL { get; set; }

        public required decimal Price { get; set; }

        public required string ConnectionType { get; set; }

        public required string WearingStyle { get; set; }

        public required string Brand { get; set; }
    }
}
