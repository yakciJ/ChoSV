using ChoSV.Models.DTOs.Common;
using ChoSV.Models.DTOs.Report;

namespace ChoSV.Services.Interfaces
{
    public interface IReportService
    {
        Task SendReportAsync(string reporterId, SendReportDTO sendReportDTO);
        Task<PagedResult<GetReportDTO>> GetAllReportsAsync(int page, int pageSize, string? status);
        Task ChangeReportStatusAsync(ChangeReportStatusDTO changeReportStatusDTO);
        Task DeleteReportAsync(int reportId);
    }
}
