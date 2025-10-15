using ChoSV.Models.DTOs.Category;

namespace ChoSV.Models.DTOs.UserViewHistory
{
    public class GetUserViewHistories
    {
        public int ProductId { get; set; }
        public required string ProductName { get; set; }
        public required string SellerId { get; set; }
        public required string SellerName { get; set; }
        public required decimal Price { get; set; }
        public required string Status { get; set; }
        public bool IsFavorited { get; set; }
        public List<CategoryDTO> Categories { get; set; } = new List<CategoryDTO>();
        public string? FirstImageUrl { get; set; }

    }
}
