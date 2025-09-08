using ChoSV.Models.DTOs.Report;
using ChoSV.Models.Entities;

namespace ChoSV.Models.Mappers
{
    public static class ReportMapper
    {
        public static GetReportDTO ToGetReportDTO(this Report report)
        {
            return new GetReportDTO
            {
                ReportId = report.ReportId,
                ReporterId = report.ReporterId,
                ReporterName = report.Reporter?.UserName ?? string.Empty,
                ReportedEntityId = report.ReportedEntityId,
                ReportedEntityType = report.ReportedEntityType,
                ReportReason = report.ReportReason,
                Status = report.Status,
                ReportedDate = report.ReportedDate,
            };
        }
    }
}
