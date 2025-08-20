namespace ChoSV.Models.DTOs.Chat
{
    public class MessageDTO
    {
        public int MessageId { get; set; }
        public required string SenderId { get; set; }
        public required string SenderUserName { get; set; }
        public required string ReceiverId { get; set; }
        public required string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsRead { get; set; }
    }
}
