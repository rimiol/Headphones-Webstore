using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Products
{
    [Key]
    public int ProductId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [Required]
    [StringLength(500)]
    public string Description { get; set; }

    [Required]
    public string ImageURL { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Required]
    [StringLength(50)]
    public string ConnectionType { get; set; }

    [Required]
    [StringLength(50)]
    public string WearingStyle { get; set; }

    [Required]
    [StringLength(50)]
    public string Brand { get; set; }
}