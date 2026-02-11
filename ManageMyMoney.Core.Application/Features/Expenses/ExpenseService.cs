using ManageMyMoney.Core.Application.Common.Interfaces;
using ManageMyMoney.Core.Application.DTOs.Common;
using ManageMyMoney.Core.Application.DTOs.Expenses;
using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.Expenses;
using ManageMyMoney.Core.Domain.Enums;
using ManageMyMoney.Core.Domain.Interfaces;
using ManageMyMoney.Core.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace ManageMyMoney.Core.Application.Features.Expenses;

public class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IExportService _exportService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<ExpenseService> _logger;

    public ExpenseService(
        IExpenseRepository expenseRepository,
        ICategoryRepository categoryRepository,
        IAccountRepository accountRepository,
        IExportService exportService,
        IFileStorageService fileStorageService,
        ILogger<ExpenseService> logger)
    {
        _expenseRepository = expenseRepository;
        _categoryRepository = categoryRepository;
        _accountRepository = accountRepository;
        _exportService = exportService;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<OperationResult<ExpenseResponse>> CreateExpenseAsync(Guid userId, CreateExpenseRequest request)
    {
        var categoryExists = await _categoryRepository.ExistsAsync(request.CategoryId);
        if (categoryExists.IsFailure || !categoryExists.Value)
            return OperationResult.Failure<ExpenseResponse>("Category not found");

        var accountExists = await _accountRepository.ExistsAsync(request.AccountId);
        if (accountExists.IsFailure || !accountExists.Value)
            return OperationResult.Failure<ExpenseResponse>("Account not found");

        var expenseResult = Expense.Create(
            request.Amount,
            request.Currency,
            request.Description,
            request.Date,
            request.CategoryId,
            request.AccountId,
            userId,
            request.SubcategoryId,
            request.PaymentMethodId,
            request.Notes,
            request.Location);

        if (expenseResult.IsFailure || expenseResult.Value == null)
            return OperationResult.Failure<ExpenseResponse>(expenseResult.Error);

        var addResult = await _expenseRepository.AddAsync(expenseResult.Value);
        if (addResult.IsFailure)
            return OperationResult.Failure<ExpenseResponse>(addResult.Error);

        // Debit from account
        var accountResult = await _accountRepository.GetByIdAsync(request.AccountId);
        if (accountResult.IsSuccess && accountResult.Value != null)
        {
            var moneyResult = Money.Create(request.Amount, request.Currency);
            if (moneyResult.IsSuccess)
            {
                accountResult.Value.Debit(moneyResult.Value!);
                await _accountRepository.UpdateAsync(accountResult.Value);
            }
        }

        _logger.LogInformation("Expense created successfully for user {UserId}", userId);

        return OperationResult.Success(MapToResponse(expenseResult.Value));
    }

    public async Task<OperationResult<ExpenseResponse>> CreateQuickExpenseAsync(Guid userId, CreateQuickExpenseRequest request)
    {
        Guid categoryId;
        
        if (request.CategoryId.HasValue)
        {
            categoryId = request.CategoryId.Value;
        }
        else
        {
            var defaultCategories = await _categoryRepository.GetDefaultCategoriesAsync();
            if (defaultCategories.IsFailure || !defaultCategories.Value!.Any())
                return OperationResult.Failure<ExpenseResponse>("No default category found. Please specify a category.");
            
            categoryId = defaultCategories.Value!.First().Id;
        }

        var accounts = await _accountRepository.GetActiveByUserAsync(userId);
        if (accounts.IsFailure || !accounts.Value!.Any())
            return OperationResult.Failure<ExpenseResponse>("No active account found");

        var defaultAccount = accounts.Value!.First();

        var createRequest = new CreateExpenseRequest
        {
            Amount = request.Amount,
            Currency = defaultAccount.Currency,
            Description = request.Description,
            Date = DateTime.UtcNow,
            CategoryId = categoryId,
            AccountId = defaultAccount.Id
        };

        return await CreateExpenseAsync(userId, createRequest);
    }

    public async Task<OperationResult<ExpenseResponse>> GetExpenseByIdAsync(Guid userId, Guid expenseId)
    {
        var result = await _expenseRepository.GetByIdAsync(expenseId);
        if (result.IsFailure || result.Value == null)
            return OperationResult.Failure<ExpenseResponse>(result.Error ?? "Expense not found");

        if (result.Value.UserId != userId)
            return OperationResult.Failure<ExpenseResponse>("Expense not found");

        return OperationResult.Success(MapToResponse(result.Value));
    }

    public async Task<OperationResult<PaginatedResponse<ExpenseResponse>>> GetExpensesAsync(Guid userId, ExpenseFilterRequest filter)
    {
        var dateRange = filter.FromDate.HasValue && filter.ToDate.HasValue
            ? DateRange.Create(filter.FromDate.Value, filter.ToDate.Value)
            : null;

        IEnumerable<Expense> expenses;

        if (dateRange?.IsSuccess == true)
        {
            var result = await _expenseRepository.GetByUserAndDateRangeAsync(userId, dateRange.Value!);
            if (result.IsFailure || result.Value == null)
                return OperationResult.Failure<PaginatedResponse<ExpenseResponse>>(result.Error ?? "Expenses fail");
            expenses = result.Value;
        }
        else
        {
            var result = await _expenseRepository.GetAllByUserAsync(userId);
            if (result.IsFailure || result.Value == null)
                return OperationResult.Failure<PaginatedResponse<ExpenseResponse>>(result.Error ?? "Expenses fail");
            expenses = result.Value;
        }

        // Apply additional filters
        if (filter.CategoryId.HasValue)
            expenses = expenses.Where(e => e.CategoryId == filter.CategoryId.Value);

        if (filter.AccountId.HasValue)
            expenses = expenses.Where(e => e.AccountId == filter.AccountId.Value);

        if (filter.MinAmount.HasValue)
            expenses = expenses.Where(e => e.Amount.Amount >= filter.MinAmount.Value);

        if (filter.MaxAmount.HasValue)
            expenses = expenses.Where(e => e.Amount.Amount <= filter.MaxAmount.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            expenses = expenses.Where(e => e.Description.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase));

        var totalCount = expenses.Count();
        var items = expenses
            .OrderByDescending(e => e.Date)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(MapToResponse);

        var response = new PaginatedResponse<ExpenseResponse>
        {
            Items = items,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalCount = totalCount
        };

        return OperationResult.Success(response);
    }

    public async Task<OperationResult<ExpenseResponse>> UpdateExpenseAsync(Guid userId, Guid expenseId, UpdateExpenseRequest request)
    {
        var expenseResult = await _expenseRepository.GetByIdAsync(expenseId);
        if (expenseResult.IsFailure || expenseResult.Value == null)
            return OperationResult.Failure<ExpenseResponse>(expenseResult.Error ?? "Expense not found");

        var expense = expenseResult.Value;
        if (expense.UserId != userId)
            return OperationResult.Failure<ExpenseResponse>("Expense not found");

        var category = await _categoryRepository.GetByIdAsync(expense.CategoryId);
        var isFixed = category.IsSuccess && category.Value!.IsFixed;

        if (request.Amount.HasValue)
        {
            var updateResult = expense.UpdateAmount(request.Amount.Value, isFixed);
            if (updateResult.IsFailure)
                return OperationResult.Failure<ExpenseResponse>(updateResult.Error);
        }

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            var updateResult = expense.UpdateDescription(request.Description);
            if (updateResult.IsFailure)
                return OperationResult.Failure<ExpenseResponse>(updateResult.Error);
        }

        if (request.CategoryId.HasValue)
        {
            var updateResult = expense.UpdateCategory(request.CategoryId.Value, request.SubcategoryId);
            if (updateResult.IsFailure)
                return OperationResult.Failure<ExpenseResponse>(updateResult.Error);
        }

        var saveResult = await _expenseRepository.UpdateAsync(expense);
        if (saveResult.IsFailure)
            return OperationResult.Failure<ExpenseResponse>(saveResult.Error);

        return OperationResult.Success(MapToResponse(expense));
    }

    public async Task<OperationResult> DeleteExpenseAsync(Guid userId, Guid expenseId)
    {
        var expenseResult = await _expenseRepository.GetByIdAsync(expenseId);
        if (expenseResult.IsFailure || expenseResult.Value == null)
            return OperationResult.Failure(expenseResult.Error ?? "Expense not found");

        if (expenseResult.Value.UserId != userId)
            return OperationResult.Failure("Expense not found");

        return await _expenseRepository.DeleteAsync(expenseId);
    }

    public async Task<OperationResult<ExpenseSummaryResponse>> GetMonthlySummaryAsync(Guid userId, int year, int month)
    {
        var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = startDate.AddMonths(1).AddTicks(-1);

        var dateRangeResult = DateRange.Create(startDate, endDate);
        if (dateRangeResult.IsFailure)
            return OperationResult.Failure<ExpenseSummaryResponse>(dateRangeResult.Error);

        var expensesResult = await _expenseRepository.GetByUserAndDateRangeAsync(userId, dateRangeResult.Value!);
        if (expensesResult.IsFailure || expensesResult.Value == null)
            return OperationResult.Failure<ExpenseSummaryResponse>(expensesResult.Error ?? "Expenses fail");

        var expenses = expensesResult.Value.ToList();
        
        if (!expenses.Any())
        {
            return OperationResult.Success(new ExpenseSummaryResponse
            {
                TotalAmount = 0,
                Currency = "USD",
                ExpenseCount = 0,
                AverageExpense = 0,
                HighestExpense = 0,
                LowestExpense = 0,
                DailyAverage = 0,
                TopCategoryName = "N/A",
                TopCategoryAmount = 0
            });
        }

        var currency = expenses.First().Amount.Currency;
        var totalAmount = expenses.Sum(e => e.Amount.Amount);
        var daysInMonth = DateTime.DaysInMonth(year, month);

        var categoryGroups = expenses
            .GroupBy(e => e.CategoryId)
            .OrderByDescending(g => g.Sum(e => e.Amount.Amount))
            .FirstOrDefault();

        var topCategoryId = categoryGroups?.Key ?? Guid.Empty;
        var topCategoryAmount = categoryGroups?.Sum(e => e.Amount.Amount) ?? 0;

        var categoryResult = topCategoryId != Guid.Empty 
            ? await _categoryRepository.GetByIdAsync(topCategoryId) 
            : null;

        var summary = new ExpenseSummaryResponse
        {
            TotalAmount = totalAmount,
            Currency = currency,
            ExpenseCount = expenses.Count,
            AverageExpense = totalAmount / expenses.Count,
            HighestExpense = expenses.Max(e => e.Amount.Amount),
            LowestExpense = expenses.Min(e => e.Amount.Amount),
            DailyAverage = totalAmount / daysInMonth,
            TopCategoryName = categoryResult?.Value?.Name ?? "Unknown",
            TopCategoryAmount = topCategoryAmount
        };

        return OperationResult.Success(summary);
    }

    public async Task<OperationResult<IEnumerable<CategoryExpenseSummary>>> GetSummaryByCategoryAsync(Guid userId, DateTime fromDate, DateTime toDate)
    {
        var dateRangeResult = DateRange.Create(fromDate, toDate);
        if (dateRangeResult.IsFailure)
            return OperationResult.Failure<IEnumerable<CategoryExpenseSummary>>(dateRangeResult.Error);

        var expensesResult = await _expenseRepository.GetByUserAndDateRangeAsync(userId, dateRangeResult.Value!);
        if (expensesResult.IsFailure || expensesResult.Value == null)
            return OperationResult.Failure<IEnumerable<CategoryExpenseSummary>>(expensesResult.Error ?? "Expenses fail");

        var expenses = expensesResult.Value.ToList();
        if (!expenses.Any())
            return OperationResult.Success<IEnumerable<CategoryExpenseSummary>>(Enumerable.Empty<CategoryExpenseSummary>());

        var totalAmount = expenses.Sum(e => e.Amount.Amount);
        var currency = expenses.First().Amount.Currency;

        var categoryGroups = expenses
            .GroupBy(e => e.CategoryId)
            .Select(g => new { CategoryId = g.Key, Total = g.Sum(e => e.Amount.Amount), Count = g.Count() })
            .OrderByDescending(g => g.Total)
            .ToList();

        var summaries = new List<CategoryExpenseSummary>();

        foreach (var group in categoryGroups)
        {
            var categoryResult = await _categoryRepository.GetByIdAsync(group.CategoryId);
            var category = categoryResult.Value;

            summaries.Add(new CategoryExpenseSummary
            {
                CategoryId = group.CategoryId,
                CategoryName = category?.Name ?? "Unknown",
                CategoryIcon = category?.Icon,
                CategoryColor = category?.Color,
                TotalAmount = group.Total,
                Currency = currency,
                ExpenseCount = group.Count,
                Percentage = Math.Round((group.Total / totalAmount) * 100, 2)
            });
        }

        return OperationResult.Success<IEnumerable<CategoryExpenseSummary>>(summaries);
    }

    public async Task<OperationResult<IEnumerable<DailyExpenseSummary>>> GetDailySummaryAsync(Guid userId, DateTime fromDate, DateTime toDate)
    {
        var dateRangeResult = DateRange.Create(fromDate, toDate);
        if (dateRangeResult.IsFailure)
            return OperationResult.Failure<IEnumerable<DailyExpenseSummary>>(dateRangeResult.Error);

        var expensesResult = await _expenseRepository.GetByUserAndDateRangeAsync(userId, dateRangeResult.Value!);
        if (expensesResult.IsFailure || expensesResult.Value == null)
            return OperationResult.Failure<IEnumerable<DailyExpenseSummary>>(expensesResult.Error ?? "Expenses fail");

        var expenses = expensesResult.Value.ToList();
        var currency = expenses.FirstOrDefault()?.Amount.Currency ?? "USD";

        var dailySummaries = expenses
            .GroupBy(e => e.Date.Date)
            .Select(g => new DailyExpenseSummary
            {
                Date = g.Key,
                TotalAmount = g.Sum(e => e.Amount.Amount),
                Currency = currency,
                ExpenseCount = g.Count()
            })
            .OrderBy(d => d.Date)
            .ToList();

        return OperationResult.Success<IEnumerable<DailyExpenseSummary>>(dailySummaries);
    }

    public async Task<OperationResult<TagResponse>> CreateTagAsync(Guid userId, CreateExpenseTagRequest request)
    {
        var tagResult = ExpenseTag.Create(request.Name, userId, request.Color);
        if (tagResult.IsFailure || tagResult.Value == null)
            return OperationResult.Failure<TagResponse>(tagResult.Error);

        // Note: Would need IExpenseTagRepository - simplified for now
        return OperationResult.Success(new TagResponse
        {
            Id = tagResult.Value.Id,
            Name = tagResult.Value.Name,
            Color = tagResult.Value.Color
        });
    }

    public Task<OperationResult<IEnumerable<TagResponse>>> GetTagsAsync(Guid userId)
    {
        // Simplified - would need IExpenseTagRepository
        return Task.FromResult(OperationResult.Success<IEnumerable<TagResponse>>(Enumerable.Empty<TagResponse>()));
    }

    public Task<OperationResult> DeleteTagAsync(Guid userId, Guid tagId)
    {
        return Task.FromResult(OperationResult.Success());
    }

    public Task<OperationResult> AddTagToExpenseAsync(Guid userId, Guid expenseId, Guid tagId)
    {
        return Task.FromResult(OperationResult.Success());
    }

    public Task<OperationResult> RemoveTagFromExpenseAsync(Guid userId, Guid expenseId, Guid tagId)
    {
        return Task.FromResult(OperationResult.Success());
    }

    public async Task<OperationResult<AttachmentResponse>> AddAttachmentAsync(Guid userId, Guid expenseId, Stream fileStream, string fileName, string contentType)
    {
        var expenseResult = await _expenseRepository.GetByIdAsync(expenseId);
        if (expenseResult.IsFailure || expenseResult.Value == null)
            return OperationResult.Failure<AttachmentResponse>(expenseResult.Error ?? "Expense not found");

        if (expenseResult.Value.UserId != userId)
            return OperationResult.Failure<AttachmentResponse>("Expense not found");

        var uploadResult = await _fileStorageService.UploadFileAsync(fileStream, fileName, contentType);
        if (uploadResult.IsFailure)
            return OperationResult.Failure<AttachmentResponse>(uploadResult.Error);

        var attachmentResult = ExpenseAttachment.Create(
            expenseId,
            fileName,
            uploadResult.Value!,
            contentType,
            fileStream.Length);

        if (attachmentResult.IsFailure || attachmentResult.Value == null)
            return OperationResult.Failure<AttachmentResponse>(attachmentResult.Error);

        expenseResult.Value.AddAttachment(attachmentResult.Value);
        await _expenseRepository.UpdateAsync(expenseResult.Value);

        return OperationResult.Success(new AttachmentResponse
        {
            Id = attachmentResult.Value!.Id,
            FileName = attachmentResult.Value!.FileName,
            FileUrl = attachmentResult.Value!.FileUrl,
            ContentType = attachmentResult.Value!.ContentType,
            FileSizeBytes = attachmentResult.Value!.FileSizeBytes,
            UploadedAt = attachmentResult.Value!.UploadedAt
        });
    }

    public Task<OperationResult> DeleteAttachmentAsync(Guid userId, Guid expenseId, Guid attachmentId)
    {
        return Task.FromResult(OperationResult.Success());
    }

    public async Task<OperationResult<RecurringExpenseResponse>> CreateRecurringExpenseAsync(Guid userId, CreateRecurringExpenseRequest request)
    {
        if (!Enum.TryParse<RecurrenceType>(request.Recurrence, true, out var recurrence))
            return OperationResult.Failure<RecurringExpenseResponse>("Invalid recurrence type");

        var result = RecurringExpense.Create(
            request.Name,
            request.Amount,
            request.Currency,
            recurrence,
            request.CategoryId,
            request.AccountId,
            userId,
            request.StartDate,
            request.DayOfMonth,
            null,
            request.EndDate,
            request.Description);

        if (result.IsFailure || result.Value == null)
            return OperationResult.Failure<RecurringExpenseResponse>(result.Error);

        // Would need IRecurringExpenseRepository
        return OperationResult.Success(new RecurringExpenseResponse
        {
            Id = result.Value!.Id,
            Name = result.Value!.Name,
            Amount = result.Value!.Amount.Amount,
            Currency = result.Value!.Amount.Currency,
            Description = result.Value!.Description,
            Recurrence = result.Value!.Recurrence.ToString(),
            DayOfMonth = result.Value!.DayOfMonth,
            CategoryId = result.Value!.CategoryId,
            CategoryName = "Unknown",
            StartDate = result.Value!.StartDate,
            EndDate = result.Value!.EndDate,
            NextDueDate = result.Value!.NextDueDate,
            IsActive = result.Value!.IsActive
        });
    }

    public Task<OperationResult<IEnumerable<RecurringExpenseResponse>>> GetRecurringExpensesAsync(Guid userId)
    {
        return Task.FromResult(OperationResult.Success<IEnumerable<RecurringExpenseResponse>>(Enumerable.Empty<RecurringExpenseResponse>()));
    }

    public Task<OperationResult> PauseRecurringExpenseAsync(Guid userId, Guid recurringExpenseId)
    {
        return Task.FromResult(OperationResult.Success());
    }

    public Task<OperationResult> ResumeRecurringExpenseAsync(Guid userId, Guid recurringExpenseId)
    {
        return Task.FromResult(OperationResult.Success());
    }

    public Task<OperationResult> DeleteRecurringExpenseAsync(Guid userId, Guid recurringExpenseId)
    {
        return Task.FromResult(OperationResult.Success());
    }

    public async Task<OperationResult<byte[]>> ExportToExcelAsync(Guid userId, DateTime fromDate, DateTime toDate)
    {
        var dateRangeResult = DateRange.Create(fromDate, toDate);
        if (dateRangeResult.IsFailure)
            return OperationResult.Failure<byte[]>(dateRangeResult.Error);

        var expensesResult = await _expenseRepository.GetByUserAndDateRangeAsync(userId, dateRangeResult.Value!);
        if (expensesResult.IsFailure)
            return OperationResult.Failure<byte[]>(expensesResult.Error);

        var exportData = expensesResult.Value!.Select(e => new
        {
            Date = e.Date.ToString("yyyy-MM-dd"),
            e.Description,
            Amount = e.Amount.Amount,
            Currency = e.Amount.Currency,
            e.Notes,
            e.Location
        });

        return await _exportService.ExportToExcelAsync(exportData, "Expenses");
    }

    public async Task<OperationResult<byte[]>> ExportToCsvAsync(Guid userId, DateTime fromDate, DateTime toDate)
    {
        var dateRangeResult = DateRange.Create(fromDate, toDate);
        if (dateRangeResult.IsFailure)
            return OperationResult.Failure<byte[]>(dateRangeResult.Error);

        var expensesResult = await _expenseRepository.GetByUserAndDateRangeAsync(userId, dateRangeResult.Value!);
        if (expensesResult.IsFailure)
            return OperationResult.Failure<byte[]>(expensesResult.Error);

        var exportData = expensesResult.Value!.Select(e => new
        {
            Date = e.Date.ToString("yyyy-MM-dd"),
            e.Description,
            Amount = e.Amount.Amount,
            Currency = e.Amount.Currency
        });

        return await _exportService.ExportToCsvAsync(exportData);
    }

    private static ExpenseResponse MapToResponse(Expense expense)
    {
        return new ExpenseResponse
        {
            Id = expense.Id,
            Amount = expense.Amount.Amount,
            Currency = expense.Amount.Currency,
            Description = expense.Description,
            Date = expense.Date,
            CategoryId = expense.CategoryId,
            CategoryName = "Unknown", // Would be populated via include/join
            SubcategoryId = expense.SubcategoryId,
            AccountId = expense.AccountId,
            AccountName = "Unknown", // Would be populated via include/join
            Notes = expense.Notes,
            Location = expense.Location,
            IsRecurring = expense.IsRecurring,
            Tags = expense.Tags.Select(t => new TagResponse
            {
                Id = t.Id,
                Name = t.Name,
                Color = t.Color
            }).ToList(),
            Attachments = expense.Attachments.Select(a => new AttachmentResponse
            {
                Id = a.Id,
                FileName = a.FileName,
                FileUrl = a.FileUrl,
                ContentType = a.ContentType,
                FileSizeBytes = a.FileSizeBytes,
                UploadedAt = a.UploadedAt
            }).ToList(),
            CreatedAt = expense.CreatedAt
        };
    }
}
