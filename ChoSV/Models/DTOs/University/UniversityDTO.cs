namespace ChoSV.Models.DTOs.University
{
    public class UniversityDTO
    {
        public int UniversityId { get; set; }
        public required string UniversityName { get; set; }
        public string? UniversityEmail { get; set; }
        public string? UniversityLogo { get; set; }
    }
}
