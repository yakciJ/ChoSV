using ChoSV.Data;
using ChoSV.Models.DTOs.Common;
using ChoSV.Models.DTOs.Notification;
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
        private readonly INotificationService _notificationService;

        public ReportService(ApplicationDBContext dbContext, INotificationService notificationService)
        {
            _dbContext = dbContext;
            _notificationService = notificationService;
        }
        // giới hạn là User, Post or Product, Comment or UserWallPost

        public async Task SendReportAsync(string reporterId, SendReportDTO sendReportDTO)
        {
            var reportType = new string[] { "User", "Product", "Comment" };
            if (!reportType.Contains(sendReportDTO.ReportedEntityType))
            {
                throw new ArgumentException("Kiểu báo cáo không hợp lệ!");
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

            // Validate status parameter
            if (!string.IsNullOrEmpty(status))
            {
                var validStatuses = new string[] { "Pending", "Approved", "Rejected" };
                if (!validStatuses.Contains(status))
                {
                    throw new ArgumentException("Trạng thái báo cáo không hợp lệ! Chỉ chấp nhận: Pending, Approved, Rejected");
                }
            }

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
            var validStatuses = new string[] { "Pending", "Approved", "Rejected" };
            if (!validStatuses.Contains(changeReportStatusDTO.Status))
            {
                throw new ArgumentException("Trạng thái báo cáo không hợp lệ! Chỉ chấp nhận: Pending, Approved, Rejected");
            }

            var report = await _dbContext.Reports
                .Include(r => r.Reporter)
                .FirstOrDefaultAsync(r => r.ReportId == changeReportStatusDTO.ReportId);
            if (report == null)
            {
                throw new ArgumentException("Báo cáo không tồn tại!");
            }

            string oldStatus = report.Status;
            report.Status = changeReportStatusDTO.Status;
            await _dbContext.SaveChangesAsync();

            // Send notifications only when status changes from Pending to Approved
            if (oldStatus == "Pending" && changeReportStatusDTO.Status == "Approved")
            {
                await SendReportNotificationsAsync(report);
            }
        }

        public async Task DeleteReportAsync(int reportId)
        {
            var report = await _dbContext.Reports.FindAsync(reportId);
            if (report == null)
            {
                throw new ArgumentException("Báo cáo không tồn tại!");
            }
            _dbContext.Reports.Remove(report);
            await _dbContext.SaveChangesAsync();
        }

        private async Task SendReportNotificationsAsync(Report report)
        {
            // Notification to the reporter (person who submitted the report)
            var reporterNotification = new SendNotificationDTO
            {
                UserId = report.ReporterId,
                Message = $"Cảm ơn bạn đã báo cáo! Chúng tôi đã xử lý báo cáo của bạn về {GetEntityTypeName(report.ReportedEntityType)}."
            };
            await _notificationService.SendNotificationAsync(reporterNotification);

            // Get the reported user ID based on entity type
            string? reportedUserId = await GetReportedUserIdAsync(report);

            if (!string.IsNullOrEmpty(reportedUserId))
            {
                // Notification to the reported user
                string message = string.Empty;
                if (report.ReportedEntityType == "User")
                {
                    message = $"Bạn đã vi phạm quy định của nền tảng và đã bị xử lý.";
                }
                else
                {
                    message = $"{GetEntityTypeName(report.ReportedEntityType)} của bạn đã vi phạm quy định của nền tảng và đã bị xử lý.";
                }
                var reportedUserNotification = new SendNotificationDTO
                {
                    UserId = reportedUserId,
                    Message = message
                };
                await _notificationService.SendNotificationAsync(reportedUserNotification);
            }
        }

        private async Task<string?> GetReportedUserIdAsync(Report report)
        {
            return report.ReportedEntityType switch
            {
                "User" => report.ReportedEntityId,
                "Product" => await _dbContext.Products
                    .AsNoTracking()
                    .Where(p => p.ProductId.ToString() == report.ReportedEntityId)
                    .Select(p => p.SellerId)
                    .FirstOrDefaultAsync(),
                "Comment" => await _dbContext.UserWallPosts
                    .AsNoTracking()
                    .Where(c => c.UserWallPostId.ToString() == report.ReportedEntityId)
                    .Select(c => c.PosterId)
                    .FirstOrDefaultAsync(),
                _ => null
            };
        }

        private static string GetEntityTypeName(string entityType)
        {
            return entityType switch
            {
                "User" => "Người dùng",
                "Product" => "Sản phẩm",
                "Comment" => "Đánh giá",
                _ => "Nội dung"
            };
        }
    }
}
