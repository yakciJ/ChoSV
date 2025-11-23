using System.ComponentModel.DataAnnotations;

namespace ChoSV.Models.DTOs.Product
{
    public class CreateProductPostDTO
    {
        [Required]
        public required string ProductName { get; set; }

        public string? ProductDescription { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public required decimal Price { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one category must be selected")]
        public List<int> CategoryIds { get; set; } = new List<int>();

        [Required]
        [MinLength(1, ErrorMessage = "At least one image is required")]
        [MaxLength(6, ErrorMessage = "Maximum 6 images allowed")]
        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}
