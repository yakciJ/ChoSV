using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChoSV.Models.Entities
{
    public class Category
    {
        public int CategoryId { get; set; }
        [Required]
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int? ParentCategoryId { get; set; }
        [InverseProperty("ParentCategory")]
        public virtual ICollection<Category>? ChildCategories { get; set; }
        [ForeignKey("ParentCategoryId")]
        [InverseProperty("ChildCategories")]
        public virtual Category? ParentCategory { get; set; }
        [InverseProperty("Categories")]
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
