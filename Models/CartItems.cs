using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Headphones_Webstore.Models
{
    public class CartItems
    {
        [Key]
        public int CartItemID { get; set; }

        [ForeignKey("Sessions")]
        public required Guid SessionID { get; set; }

        [ForeignKey("Products")]
        public required int ProductID { get; set; }

        public required int Quantity { get; set; }

        public required DateTime AddedAt { get; set; }
    }
}
