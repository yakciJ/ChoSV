using System.ComponentModel.DataAnnotations;

namespace ChoSV.Models.DTOs.Chat
{
    public class SendMessageDTO
    {
        [Required]
        public required string ReceiverId { get; set; }
        [Required]
        [StringLength(1000, ErrorMessage = "Tin nhắn không vượt quá 1000 ký tự!")]
        public required string Content { get; set; }
    }
}
