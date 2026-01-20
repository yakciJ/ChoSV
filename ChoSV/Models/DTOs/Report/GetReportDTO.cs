using System.ComponentModel.DataAnnotations;

namespace ChoSV.Models.DTOs.Report
{
    public class GetReportDTO
    {
        [Required]
        public int ReportId { get; set; }
        [Required]
        public required string ReporterId { get; set; }
        [Required]
        public required string ReporterName { get; set; }
        [Required]
        public required string ReportedEntityId { get; set; }
        [Required]
        [StringLength(50)]
        public required string ReportedEntityType { get; set; }
        [Required]
        [StringLength(500)]
        public required string ReportReason { get; set; }
        public DateTime ReportedDate { get; set; }
        [Required]
        [StringLength(20)]
        public required string Status { get; set; }
    }
}
