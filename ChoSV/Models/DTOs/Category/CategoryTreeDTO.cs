namespace ChoSV.Models.DTOs.Category
{
    public class CategoryTreeDTO
    {
        public int CategoryId { get; set; }
        public string? Name { get; set; }
        public string? ImageUrl { get; set; }
        public List<CategoryTreeDTO> Childs { get; set; } = new List<CategoryTreeDTO>();
    }
}
