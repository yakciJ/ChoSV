using System.ComponentModel.DataAnnotations;

namespace ChoSV.Models.DTOs.Image
{
    public class UploadImageDTO
    {
        [Required]
        public required IFormFile File { get; set; }
    }
}
