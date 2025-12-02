using ChoSV.Data;
using ChoSV.Models.DTOs.Common;
using ChoSV.Models.DTOs.Report;
using ChoSV.Models.Entities;
using ChoSV.Models.Mappers;
using ChoSV.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChoSV.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDBContext _dbContext;
        public ReportService(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }
        // giới hạn là Message, User, Post or Product, Comment or UserWallPost

        public async Task SendReportAsync(string reporterId, SendReportDTO sendReportDTO)
        {
            var reportType = new string[] { "User", "Product", "Comment", "Message" };
            if (!reportType.Contains(sendReportDTO.ReportedEntityType))
            {
                throw new ArgumentException("Kiểu báo cáo không hợp lệ!");
            }
            var report = await _dbContext.Reports
                .Where(r => r.ReporterId == reporterId && r.ReportedEntityId == sendReportDTO.ReportedEntityId && r.ReportedEntityType == sendReportDTO.ReportedEntityType)
                .FirstOrDefaultAsync();
            if (report != null)
            {
                throw new ArgumentException("Bạn đã báo cáo rồi!");
            }
            var newReport = new Report
            {
                ReporterId = reporterId,
                ReportedEntityId = sendReportDTO.ReportedEntityId,
                ReportedEntityType = sendReportDTO.ReportedEntityType,
                ReportReason = sendReportDTO.ReportReason,
                ReportedDate = DateTime.UtcNow,
                Status = "Pending"
            };
            _dbContext.Reports.Add(newReport);
            await _dbContext.SaveChangesAsync();
        }
        public async Task<PagedResult<GetReportDTO>> GetAllReportsAsync(int page = 1, int pageSize = 10, string? status = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = _dbContext.Reports
                .AsNoTracking()
                .Include(r => r.Reporter)
                .Where(r => status == null || r.Status == status)
                .OrderByDescending(r => r.ReportedDate);

            var totalCount = await query.CountAsync();

            var reports = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var reportDTOs = reports.Select(r => r.ToGetReportDTO()).ToList();

            return new PagedResult<GetReportDTO>
            {
                Items = reportDTOs,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
            };
        }
        public async Task ChangeReportStatusAsync(ChangeReportStatusDTO changeReportStatusDTO)
        {
            var report = await _dbContext.Reports.FindAsync(changeReportStatusDTO.ReportId);
            if (report == null)
            {
                throw new ArgumentException("Báo cáo không tồn tại!");
            }
            report.Status = changeReportStatusDTO.Status;
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteReportAsync(string reportId)
        {
            var report = await _dbContext.Reports.FindAsync(reportId);
            if (report == null)
            {
                throw new ArgumentException("Báo cáo không tồn tại!");
            }
            _dbContext.Reports.Remove(report);
            await _dbContext.SaveChangesAsync();
        }
    }
}
