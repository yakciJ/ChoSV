using ChoSV.Models.DTOs.Report;
using ChoSV.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChoSV.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        public ReportController(IReportService reportService) => _reportService = reportService;

        [HttpPost]
        [Authorize(Policy = "UserPolicy")]
        public async Task<IActionResult> SendReportAsync(SendReportDTO sendReportDTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Không thể xác định người dùng!");
            }
            await _reportService.SendReportAsync(userId, sendReportDTO);
            return Ok(new { message = "Send report successfully" });
        }
        [HttpGet]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> GetAllReportsASync([FromQuery] int page = 1, [FromQuery] int pageSize = 10, string? status = null)
        {
            var result = await _reportService.GetAllReportsAsync(page, pageSize, status);
            return Ok(result);
        }

        [HttpPut]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> ChangeReportStatus(ChangeReportStatusDTO changeReportStatusDTO)
        {
            await _reportService.ChangeReportStatusAsync(changeReportStatusDTO);
            return Ok(new { message = "Change report status successfully" });
        }

        [HttpDelete("{reportId}")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> DeleteReportAsync(string reportId)
        {
            await _reportService.DeleteReportAsync(reportId);
            return Ok(new { message = "Delete report successfully" });
        }
    }
}
