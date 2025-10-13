namespace ChoSV.Models.DTOs.Product
{
    public class ProductFilterDTO
    {
        public int? CategoryId { get; set; }
        public bool IsFavorite { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool HasValue()
        {
            return CategoryId.HasValue
                || IsFavorite
                || MinPrice.HasValue
                || MaxPrice.HasValue;
        }
    }
}
