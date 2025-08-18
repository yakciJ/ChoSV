using ChoSV.Models.DTOs.Category;

namespace ChoSV.Models.DTOs.Product
{
    public class ProductListItemDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string SellerName { get; set; } = string.Empty;
        public string? FirstImageUrl { get; set; }
        public bool IsFavorited { get; set; }
        public int FavoriteCount { get; set; }
        public List<CategoryDTO> Categories { get; set; } = new List<CategoryDTO>(); 

    }
}
