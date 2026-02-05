using ManageMyMoney.Core.Application.DTOs.Common;
using ManageMyMoney.Core.Application.DTOs.Expenses;
using ManageMyMoney.Core.Application.Features.Expenses;
using ManageMyMoney.Presentation.Api.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManageMyMoney.Presentation.Api.Controllers;

[Authorize]
public class ExpensesController : BaseApiController
{
    private readonly IExpenseService _expenseService;

    public ExpensesController(IExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ExpenseResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateExpense([FromBody] CreateExpenseRequest request)
    {
        var result = await _expenseService.CreateExpenseAsync(CurrentUserId, request);
        return HandleCreated(result, nameof(GetExpenseById), new { id = result.Value?.Id });
    }

    [HttpPost("quick")]
    [ProducesResponseType(typeof(ApiResponse<ExpenseResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateQuickExpense([FromBody] CreateQuickExpenseRequest request)
    {
        var result = await _expenseService.CreateQuickExpenseAsync(CurrentUserId, request);
        return HandleCreated(result, nameof(GetExpenseById), new { id = result.Value?.Id });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ExpenseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExpenseById(Guid id)
    {
        var result = await _expenseService.GetExpenseByIdAsync(CurrentUserId, id);
        return HandleNotFound(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<ExpenseResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpenses([FromQuery] ExpenseFilterRequest filter)
    {
        var result = await _expenseService.GetExpensesAsync(CurrentUserId, filter);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ExpenseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateExpense(Guid id, [FromBody] UpdateExpenseRequest request)
    {
        var result = await _expenseService.UpdateExpenseAsync(CurrentUserId, id, request);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteExpense(Guid id)
    {
        var result = await _expenseService.DeleteExpenseAsync(CurrentUserId, id);
        return HandleResult(result, "Expense deleted successfully");
    }

    [HttpGet("summary/monthly")]
    [ProducesResponseType(typeof(ApiResponse<ExpenseSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMonthlySummary([FromQuery] int year, [FromQuery] int month)
    {
        var result = await _expenseService.GetMonthlySummaryAsync(CurrentUserId, year, month);
        return HandleResult(result);
    }

    [HttpGet("summary/category")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryExpenseSummary>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummaryByCategory([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        var result = await _expenseService.GetSummaryByCategoryAsync(CurrentUserId, fromDate, toDate);
        return HandleResult(result);
    }

    [HttpGet("summary/daily")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DailyExpenseSummary>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDailySummary([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        var result = await _expenseService.GetDailySummaryAsync(CurrentUserId, fromDate, toDate);
        return HandleResult(result);
    }

    [HttpGet("tags")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TagResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTags()
    {
        var result = await _expenseService.GetTagsAsync(CurrentUserId);
        return HandleResult(result);
    }

    [HttpPost("tags")]
    [ProducesResponseType(typeof(ApiResponse<TagResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateTag([FromBody] CreateExpenseTagRequest request)
    {
        var result = await _expenseService.CreateTagAsync(CurrentUserId, request);
        return HandleResult(result);
    }

    [HttpDelete("tags/{tagId:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteTag(Guid tagId)
    {
        var result = await _expenseService.DeleteTagAsync(CurrentUserId, tagId);
        return HandleResult(result, "Tag deleted successfully");
    }

    [HttpPost("{expenseId:guid}/attachments")]
    [ProducesResponseType(typeof(ApiResponse<AttachmentResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddAttachment(Guid expenseId, IFormFile file)
    {
        using var stream = file.OpenReadStream();
        var result = await _expenseService.AddAttachmentAsync(CurrentUserId, expenseId, stream, file.FileName, file.ContentType);
        return HandleResult(result);
    }

    [HttpDelete("{expenseId:guid}/attachments/{attachmentId:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteAttachment(Guid expenseId, Guid attachmentId)
    {
        var result = await _expenseService.DeleteAttachmentAsync(CurrentUserId, expenseId, attachmentId);
        return HandleResult(result, "Attachment deleted successfully");
    }

    [HttpPost("recurring")]
    [ProducesResponseType(typeof(ApiResponse<RecurringExpenseResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateRecurringExpense([FromBody] CreateRecurringExpenseRequest request)
    {
        var result = await _expenseService.CreateRecurringExpenseAsync(CurrentUserId, request);
        return HandleResult(result);
    }

    [HttpGet("recurring")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RecurringExpenseResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecurringExpenses()
    {
        var result = await _expenseService.GetRecurringExpensesAsync(CurrentUserId);
        return HandleResult(result);
    }

    [HttpGet("export/excel")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportToExcel([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        var result = await _expenseService.ExportToExcelAsync(CurrentUserId, fromDate, toDate);
        
        if (result.IsFailure)
            return BadRequest(ApiResponse.Fail(result.Error));

        return File(result.Value!, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
            $"expenses_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.xlsx");
    }

    [HttpGet("export/csv")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportToCsv([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        var result = await _expenseService.ExportToCsvAsync(CurrentUserId, fromDate, toDate);

        if (result.IsFailure)
            return BadRequest(ApiResponse.Fail(result.Error));

        return File(result.Value!, "text/csv", $"expenses_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.csv");
    }
}
