using ManageMyMoney.Core.Application.DTOs.Reports;
using ManageMyMoney.Core.Application.Features.Reports;
using ManageMyMoney.Presentation.Api.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManageMyMoney.Presentation.Api.Controllers;

[Authorize]
public class ReportsController : BaseApiController
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<FinancialSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFinancialSummary([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        var result = await _reportService.GetFinancialSummaryAsync(CurrentUserId, fromDate, toDate);
        return HandleResult(result);
    }

    [HttpGet("monthly")]
    [ProducesResponseType(typeof(ApiResponse<MonthlyReportResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMonthlyReport([FromQuery] int year, [FromQuery] int month)
    {
        var result = await _reportService.GetMonthlyReportAsync(CurrentUserId, year, month);
        return HandleResult(result);
    }

    [HttpGet("yearly")]
    [ProducesResponseType(typeof(ApiResponse<YearlyReportResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetYearlyReport([FromQuery] int year)
    {
        var result = await _reportService.GetYearlyReportAsync(CurrentUserId, year);
        return HandleResult(result);
    }

    [HttpPost("comparison")]
    [ProducesResponseType(typeof(ApiResponse<ComparisonReportResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComparisonReport([FromBody] ComparisonReportRequest request)
    {
        var result = await _reportService.GetComparisonReportAsync(CurrentUserId, request);
        return HandleResult(result);
    }

    [HttpGet("comparison/month-over-month")]
    [ProducesResponseType(typeof(ApiResponse<ComparisonReportResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMonthOverMonthComparison([FromQuery] int year, [FromQuery] int month)
    {
        var result = await _reportService.GetMonthOverMonthComparisonAsync(CurrentUserId, year, month);
        return HandleResult(result);
    }

    [HttpGet("comparison/year-over-year")]
    [ProducesResponseType(typeof(ApiResponse<ComparisonReportResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetYearOverYearComparison([FromQuery] int year)
    {
        var result = await _reportService.GetYearOverYearComparisonAsync(CurrentUserId, year);
        return HandleResult(result);
    }

    [HttpGet("trends/expenses")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MonthlyTrendItem>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpenseTrends([FromQuery] int months = 12)
    {
        var result = await _reportService.GetExpenseTrendsAsync(CurrentUserId, months);
        return HandleResult(result);
    }

    [HttpGet("trends/income")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MonthlyTrendItem>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIncomeTrends([FromQuery] int months = 12)
    {
        var result = await _reportService.GetIncomeTrendsAsync(CurrentUserId, months);
        return HandleResult(result);
    }

    [HttpGet("top-categories")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryBreakdownItem>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopExpenseCategories(
        [FromQuery] DateTime fromDate, 
        [FromQuery] DateTime toDate, 
        [FromQuery] int top = 5)
    {
        var result = await _reportService.GetTopExpenseCategoriesAsync(CurrentUserId, fromDate, toDate, top);
        return HandleResult(result);
    }

    [HttpPost("export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportReport([FromBody] ExportReportRequest request)
    {
        var result = await _reportService.ExportReportAsync(CurrentUserId, request);

        if (result.IsFailure)
            return BadRequest(ApiResponse.Fail(result.Error));

        var contentType = request.ExportFormat.ToLower() switch
        {
            "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "pdf" => "application/pdf",
            "csv" => "text/csv",
            _ => "application/octet-stream"
        };

        var extension = request.ExportFormat.ToLower() switch
        {
            "excel" => "xlsx",
            "pdf" => "pdf",
            "csv" => "csv",
            _ => "bin"
        };

        return File(result.Value!, contentType, $"report_{DateTime.UtcNow:yyyyMMdd}.{extension}");
    }

    [HttpPost("send-monthly-email")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SendMonthlyReportEmail([FromQuery] int year, [FromQuery] int month)
    {
        var result = await _reportService.SendMonthlyReportEmailAsync(CurrentUserId, year, month);
        return HandleResult(result, "Monthly report email sent");
    }
}
