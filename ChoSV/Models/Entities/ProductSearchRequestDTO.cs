using ChoSV.Models.DTOs.Product;

namespace ChoSV.Models.Entities
{
    public class ProductSearchRequestDTO
    {
        public string? Search { get; set; }
        public ProductFilterDTO? Filters { get; set; }
    }
}
