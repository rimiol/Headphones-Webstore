using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Headphones_Webstore.Models
{
    public class Sessions
    {
        [Key]
        public required Guid SessionID { get; set; }

        public required DateTime CreatedAt { get; set; }

        // Коллекция связанных CartItems
        public ICollection<CartItems> CartItems { get; set; } = new List<CartItems>(); 
    }
}