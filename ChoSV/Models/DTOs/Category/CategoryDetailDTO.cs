using ChoSV.Models.DTOs.Common;
using ChoSV.Models.DTOs.Product;

namespace ChoSV.Models.DTOs.Category
{
    public class CategoryDetailDTO
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int ProductCount { get; set; }
        public PagedResult<ProductListItemDTO> Products { get; set; } = new();

    }
}
