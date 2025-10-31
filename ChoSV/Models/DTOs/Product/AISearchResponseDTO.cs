namespace ChoSV.Models.DTOs.Product
{
    public class AISearchResponseDTO
    {
        public List<int> ProductIds { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}