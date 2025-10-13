namespace ChoSV.Models.DTOs.Product
{
    public class AISearchResponseDTO
    {
        public string Query { get; set; } = string.Empty;
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
        public List<int> Results { get; set; } = new();
    }
}