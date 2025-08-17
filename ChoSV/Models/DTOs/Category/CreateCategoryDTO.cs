namespace ChoSV.Models.DTOs.Category
{
    public class CreateCategoryDTO
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public int? ParentCategoryId { get; set; }
    }
}
