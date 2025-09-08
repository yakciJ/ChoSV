using System.ComponentModel.DataAnnotations;

namespace ChoSV.Models.DTOs.Report
{
    public class ChangeReportStatusDTO
    {
        [Required]
        public int ReportId { get; set; }
        [Required]
        public required string Status { get; set; }
    }
}
