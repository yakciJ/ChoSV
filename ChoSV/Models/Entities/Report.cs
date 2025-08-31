using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChoSV.Models.Entities
{
    public class Report
    {
        public int ReportId { get; set; }
        [Required]
        public required string ReporterId { get; set; }
        [Required]
        public required int ReportedEntityId { get; set; }
        [Required]
        [StringLength(50)]
        public required string ReportedEntityType { get; set; }
        [Required]
        [StringLength(500)]
        public required string ReportReason { get; set; }
        public DateTime ReportedDate { get; set; } = DateTime.UtcNow;
        [StringLength(20)]
        public string Status { get; set; } = "Pending";
        [ForeignKey("ReporterId")]
        [InverseProperty("Reports")]
        public virtual User Reporter { get; set; } = null!;
    }
}
