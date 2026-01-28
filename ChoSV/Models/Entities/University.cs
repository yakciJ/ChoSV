using System.ComponentModel.DataAnnotations;

namespace ChoSV.Models.Entities
{
    public class University
    {
        public int UniversityId { get; set; }
        [Required]
        public required string UniversityName { get; set; }
        public string? UniversityEmail { get; set; }
        public string? UniversityLogo { get; set; }
    }
}
