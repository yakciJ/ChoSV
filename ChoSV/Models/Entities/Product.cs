using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChoSV.Models.Entities
{
    public class Product
    {
        public int ProductId { get; set; }
        [Required]
        public required string SellerId { get; set; }
        [Required]
        public required string ProductName { get; set; }
        public string? ProductDescription { get; set; }
        public required decimal Price { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        // category
        [InverseProperty("Products")]
        public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
        [InverseProperty("Product")]
        public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
        [InverseProperty("Product")]
        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        [ForeignKey("SellerId")]
        [InverseProperty("Products")]
        public virtual User Seller { get; set; } = null!;
    }
}
