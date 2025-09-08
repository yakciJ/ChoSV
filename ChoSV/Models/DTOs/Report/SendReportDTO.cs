using System.ComponentModel.DataAnnotations;

namespace ChoSV.Models.DTOs.Report
{
    public class SendReportDTO
    {
        [Required]
        public required int ReportedEntityId { get; set; }
        [Required]
        [StringLength(50)] // giới hạn là Message, User, Post or Product, Comment or UserWallPost
        public required string ReportedEntityType { get; set; }
        [Required]
        [StringLength(500)]
        public required string ReportReason { get; set; }
    }
}
