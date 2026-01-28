using System.ComponentModel.DataAnnotations;

namespace ChoSV.Models.DTOs.University
{
    public class AddUniversityDTO
    {
        [Required]
        public required string UniversityName { get; set; }
        public string? UniversityEmail { get; set; }
        public string? UniversityLogo { get; set; }
    }
}
