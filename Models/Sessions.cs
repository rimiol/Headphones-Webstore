using System.ComponentModel.DataAnnotations;

namespace Headphones_Webstore.Models
{
    public class Sessions
    {
        [Key]
        public required Guid SessionID { get; set; }

        public required DateTime CreatedAt {  get; set; } 
    }
}
