using ChoSV.Models.DTOs.Category;

namespace ChoSV.Models.DTOs.Product
{
    public class ProductDetailListDTO
    {
        public int ProductId { get; set; }
        public required string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public string? ProductDescription { get; set; }
        public required string Status { get; set; }
        public required string SellerId { get; set; }
        public required string SellerName { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<CategoryDTO> Categories { get; set; } = new List<CategoryDTO>();
        public string? FirstImageUrl { get; set; }

    }
}
