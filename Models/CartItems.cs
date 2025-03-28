using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Headphones_Webstore.Models
{
    public class CartItems
    {
        [Key]
        public int CartItemID { get; set; }

        [ForeignKey("Session")]
        public required Guid SessionID { get; set; }

        // Навигационное свойство
        public Sessions Session { get; set; }

        [ForeignKey("Product")]
        public required int ProductID { get; set; }

        // Навигационное свойство
        public Products Product { get; set; }

        public required int Quantity { get; set; }
        public required DateTime AddedAt { get; set; }
    }
}