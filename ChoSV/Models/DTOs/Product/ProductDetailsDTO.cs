namespace ChoSV.Models.DTOs.Product
{
    public class ProductDetailsDTO
    {
        public int ProductId { get; set; }
        public required string ProductName { get; set; }
        public required string SellerId { get; set; }
        public required string SellerName { get; set; }
        public string? SellerAddress { get; set; }
        public string? SellerPhone { get; set; }
        public required string ProductDescription { get; set; }
        public required decimal Price { get; set; }
        public required string Status { get; set; }
        public DateTime CreatedDate { get; set; }

        public string? SellerFullName { get; set; }
        public string? SellerAvatarImage { get; set; }
        public string? SellerEmail { get; set; }
        public DateTime SellerJoinedDate { get; set; }

        public List<string> ProductImages { get; set; } = new List<string>();
        public int FavoriteCount { get; set; }
        public bool IsFavorite { get; set; }

        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public int? ChildCategoryId { get; set; }
        public string? ChildCategoryName { get; set; }
    }
}
