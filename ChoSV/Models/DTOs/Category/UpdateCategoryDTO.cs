namespace ChoSV.Models.DTOs.Category
{
    public class UpdateCategoryDTO
    {
        public int CategoryId { get; set; }
        public required string Name { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public int? ParentCategoryId { get; set; }
    }
}
