using ChoSV.Models.DTOs.Common;
using ChoSV.Models.DTOs.Report;

namespace ChoSV.Services.Interfaces
{
    public interface IReportService
    {
        Task SendReportAsync(string reporterId, SendReportDTO sendReportDTO);
        Task<PagedResult<GetReportDTO>> GetAllReportsAsync(int page = 1, int pageSize = 10, string? status = null);
        Task ChangeReportStatusAsync(ChangeReportStatusDTO changeReportStatusDTO);
        Task DeleteReportAsync(int reportId);
    }
}
