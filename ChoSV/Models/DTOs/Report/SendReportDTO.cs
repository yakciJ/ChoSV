using System.ComponentModel.DataAnnotations;

namespace ChoSV.Models.DTOs.Report
{
    public class SendReportDTO
    {
        [Required]
        public required string ReportedEntityId { get; set; }
        [Required]
        [StringLength(50)] // giới hạn là User, Post or Product, Comment or UserWallPost
        public required string ReportedEntityType { get; set; }
        [Required]
        [StringLength(500)]
        public required string ReportReason { get; set; }
    }
}
