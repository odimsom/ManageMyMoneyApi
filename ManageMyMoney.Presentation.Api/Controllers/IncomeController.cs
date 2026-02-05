using ManageMyMoney.Core.Application.DTOs.Common;
using ManageMyMoney.Core.Application.DTOs.Income;
using ManageMyMoney.Core.Application.Features.Income;
using ManageMyMoney.Presentation.Api.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManageMyMoney.Presentation.Api.Controllers;

[Authorize]
public class IncomeController : BaseApiController
{
    private readonly IIncomeService _incomeService;

    public IncomeController(IIncomeService incomeService)
    {
        _incomeService = incomeService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<IncomeResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateIncome([FromBody] CreateIncomeRequest request)
    {
        var result = await _incomeService.CreateIncomeAsync(CurrentUserId, request);
        return HandleCreated(result, nameof(GetIncomeById), new { id = result.Value?.Id });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IncomeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetIncomeById(Guid id)
    {
        var result = await _incomeService.GetIncomeByIdAsync(CurrentUserId, id);
        return HandleNotFound(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<IncomeResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIncomes(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        DateRangeRequest? dateRange = fromDate.HasValue && toDate.HasValue
            ? new DateRangeRequest { FromDate = fromDate.Value, ToDate = toDate.Value }
            : null;

        var result = await _incomeService.GetIncomesAsync(CurrentUserId, dateRange, pageNumber, pageSize);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IncomeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateIncome(Guid id, [FromBody] UpdateIncomeRequest request)
    {
        var result = await _incomeService.UpdateIncomeAsync(CurrentUserId, id, request);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteIncome(Guid id)
    {
        var result = await _incomeService.DeleteIncomeAsync(CurrentUserId, id);
        return HandleResult(result, "Income deleted successfully");
    }

    [HttpGet("summary/monthly")]
    [ProducesResponseType(typeof(ApiResponse<IncomeSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMonthlySummary([FromQuery] int year, [FromQuery] int month)
    {
        var result = await _incomeService.GetMonthlySummaryAsync(CurrentUserId, year, month);
        return HandleResult(result);
    }

    // Income Sources
    [HttpPost("sources")]
    [ProducesResponseType(typeof(ApiResponse<IncomeSourceResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateIncomeSource([FromBody] CreateIncomeSourceRequest request)
    {
        var result = await _incomeService.CreateIncomeSourceAsync(CurrentUserId, request);
        return HandleResult(result);
    }

    [HttpGet("sources")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<IncomeSourceResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIncomeSources()
    {
        var result = await _incomeService.GetIncomeSourcesAsync(CurrentUserId);
        return HandleResult(result);
    }

    [HttpPut("sources/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IncomeSourceResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateIncomeSource(Guid id, [FromBody] CreateIncomeSourceRequest request)
    {
        var result = await _incomeService.UpdateIncomeSourceAsync(CurrentUserId, id, request);
        return HandleResult(result);
    }

    [HttpDelete("sources/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteIncomeSource(Guid id)
    {
        var result = await _incomeService.DeleteIncomeSourceAsync(CurrentUserId, id);
        return HandleResult(result, "Income source deleted");
    }

    [HttpGet("export/excel")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportToExcel([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        var result = await _incomeService.ExportToExcelAsync(CurrentUserId, fromDate, toDate);

        if (result.IsFailure)
            return BadRequest(ApiResponse.Fail(result.Error));

        return File(result.Value!, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"income_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.xlsx");
    }
}
