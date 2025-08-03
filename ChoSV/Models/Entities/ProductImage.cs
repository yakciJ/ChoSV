using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChoSV.Models.Entities
{
    public class ProductImage
    {
        public int ProductImageId { get; set; }
        public int ProductId { get; set; }
        [Required]
        public required string ImageUrl { get; set; }
        [ForeignKey("ProductId")]
        [InverseProperty("ProductImages")]
        public virtual Product? Product { get; set; }
    }
}
