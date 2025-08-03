using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChoSV.Models.Entities
{
    public class Message
    {
        public int Id { get; set; }
        [Required]
        public required string SenderId { get; set; }
        [Required]
        public required string ReceiverId { get; set; }
        [Required]
        public required string Content { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        [ForeignKey("SenderId")]
        [InverseProperty("SentMessages")]
        public User? Sender { get; set; }
        [ForeignKey("ReceiverId")]
        [InverseProperty("ReceivedMessages")]
        public User? Receiver { get; set; }
    }
}
