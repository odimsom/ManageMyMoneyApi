using ManageMyMoney.Core.Application.Common.Interfaces;
using ManageMyMoney.Core.Application.DTOs.Common;
using ManageMyMoney.Core.Application.DTOs.Income;
using ManageMyMoney.Core.Domain.Common;
using IncomeEntity = ManageMyMoney.Core.Domain.Entities.Income.Income;
using ManageMyMoney.Core.Domain.Entities.Income;
using ManageMyMoney.Core.Domain.Interfaces;
using ManageMyMoney.Core.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace ManageMyMoney.Core.Application.Features.Income;

public class IncomeService : IIncomeService
{
    private readonly IIncomeRepository _incomeRepository;
    private readonly IIncomeSourceRepository _incomeSourceRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IRecurringIncomeRepository _recurringIncomeRepository;
    private readonly IExportService _exportService;
    private readonly ILogger<IncomeService> _logger;

    public IncomeService(
        IIncomeRepository incomeRepository,
        IIncomeSourceRepository incomeSourceRepository,
        IAccountRepository accountRepository,
        IRecurringIncomeRepository recurringIncomeRepository,
        IExportService exportService,
        ILogger<IncomeService> logger)
    {
        _incomeRepository = incomeRepository;
        _incomeSourceRepository = incomeSourceRepository;
        _accountRepository = accountRepository;
        _recurringIncomeRepository = recurringIncomeRepository;
        _exportService = exportService;
        _logger = logger;
    }

    public async Task<OperationResult<IncomeResponse>> CreateIncomeAsync(Guid userId, CreateIncomeRequest request)
    {
        var sourceExists = await _incomeSourceRepository.ExistsAsync(request.IncomeSourceId);
        if (sourceExists.IsFailure || !sourceExists.Value)
            return OperationResult.Failure<IncomeResponse>("Income source not found");

        var accountExists = await _accountRepository.ExistsAsync(request.AccountId);
        if (accountExists.IsFailure || !accountExists.Value)
            return OperationResult.Failure<IncomeResponse>("Account not found");

        var incomeResult = IncomeEntity.Create(
            request.Amount,
            request.Currency,
            request.Description,
            request.Date,
            request.IncomeSourceId,
            request.AccountId,
            userId,
            request.Notes);

        if (incomeResult.IsFailure || incomeResult.Value == null)
            return OperationResult.Failure<IncomeResponse>(incomeResult.Error);

        var addResult = await _incomeRepository.AddAsync(incomeResult.Value);
        if (addResult.IsFailure)
            return OperationResult.Failure<IncomeResponse>(addResult.Error);

        // Credit to account
        var accountResult = await _accountRepository.GetByIdAsync(request.AccountId);
        if (accountResult.IsSuccess && accountResult.Value != null)
        {
            var moneyResult = Money.Create(request.Amount, request.Currency);
            if (moneyResult.IsSuccess)
            {
                accountResult.Value.Credit(moneyResult.Value!);
                await _accountRepository.UpdateAsync(accountResult.Value);
            }
        }

        _logger.LogInformation("Income created successfully for user {UserId}", userId);

        return OperationResult.Success(await MapToResponseAsync(incomeResult.Value));
    }

    public async Task<OperationResult<IncomeResponse>> GetIncomeByIdAsync(Guid userId, Guid incomeId)
    {
        var result = await _incomeRepository.GetByIdAsync(incomeId);
        if (result.IsFailure || result.Value == null)
            return OperationResult.Failure<IncomeResponse>(result.Error ?? "Income not found");

        if (result.Value.UserId != userId)
            return OperationResult.Failure<IncomeResponse>("Income not found");

        return OperationResult.Success(await MapToResponseAsync(result.Value));
    }

    public async Task<OperationResult<PaginatedResponse<IncomeResponse>>> GetIncomesAsync(Guid userId, DateRangeRequest? dateRangeRequest, int pageNumber = 1, int pageSize = 20)
    {
        IEnumerable<IncomeEntity> incomes;

        if (dateRangeRequest != null)
        {
            var dateRangeResult = DateRange.Create(dateRangeRequest.FromDate, dateRangeRequest.ToDate);
            if (dateRangeResult.IsFailure)
                return OperationResult.Failure<PaginatedResponse<IncomeResponse>>(dateRangeResult.Error);

            var result = await _incomeRepository.GetByUserAndDateRangeAsync(userId, dateRangeResult.Value!);
            if (result.IsFailure || result.Value == null)
                return OperationResult.Failure<PaginatedResponse<IncomeResponse>>(result.Error ?? "Incomes fail");
            incomes = result.Value;
        }
        else
        {
            var result = await _incomeRepository.GetAllByUserAsync(userId);
            if (result.IsFailure || result.Value == null)
                return OperationResult.Failure<PaginatedResponse<IncomeResponse>>(result.Error ?? "Incomes fail");
            incomes = result.Value;
        }

        var totalCount = incomes.Count();
        var items = incomes
            .OrderByDescending(i => i.Date)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        var responseItems = new List<IncomeResponse>();
        foreach (var item in items)
        {
            responseItems.Add(await MapToResponseAsync(item));
        }

        var response = new PaginatedResponse<IncomeResponse>
        {
            Items = responseItems,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return OperationResult.Success(response);
    }

    public async Task<OperationResult<IncomeResponse>> UpdateIncomeAsync(Guid userId, Guid incomeId, UpdateIncomeRequest request)
    {
        var incomeResult = await _incomeRepository.GetByIdAsync(incomeId);
        if (incomeResult.IsFailure || incomeResult.Value == null)
            return OperationResult.Failure<IncomeResponse>(incomeResult.Error ?? "Income not found");

        var income = incomeResult.Value;
        if (income.UserId != userId)
            return OperationResult.Failure<IncomeResponse>("Income not found");

        // Keep track of old values for balance synchronization
        var oldAmount = income.Amount.Amount;
        var oldAccountId = income.AccountId;
        var currency = income.Amount.Currency;

        if (request.Amount.HasValue)
        {
            var updateResult = income.UpdateAmount(request.Amount.Value);
            if (updateResult.IsFailure)
                return OperationResult.Failure<IncomeResponse>(updateResult.Error);
        }

        if (request.Description != null)
        {
            var updateResult = income.UpdateDescription(request.Description);
            if (updateResult.IsFailure)
                return OperationResult.Failure<IncomeResponse>(updateResult.Error);
        }

        if (request.Date.HasValue)
        {
            income.UpdateDate(request.Date.Value);
        }

        if (request.IncomeSourceId.HasValue)
        {
            var sourceExists = await _incomeSourceRepository.ExistsAsync(request.IncomeSourceId.Value);
            if (sourceExists.IsFailure || !sourceExists.Value)
                return OperationResult.Failure<IncomeResponse>("Income source not found");

            income.UpdateIncomeSource(request.IncomeSourceId.Value);
        }

        if (request.Notes != null)
        {
            income.UpdateNotes(request.Notes);
        }

        // Account Change Logic
        if (request.AccountId.HasValue && request.AccountId.Value != oldAccountId)
        {
            // 1. Debit from old account (reverse credit)
            var oldAccount = await _accountRepository.GetByIdAsync(oldAccountId);
            if (oldAccount.IsSuccess && oldAccount.Value != null)
            {
                var oldMoney = Money.Create(oldAmount, currency);
                if (oldMoney.IsSuccess)
                {
                    oldAccount.Value.Debit(oldMoney.Value!);
                    await _accountRepository.UpdateAsync(oldAccount.Value);
                }
            }

            // 2. Update to new account
            income.UpdateAccount(request.AccountId.Value);

            // 3. Credit to new account
            var newAccount = await _accountRepository.GetByIdAsync(request.AccountId.Value);
            if (newAccount.IsSuccess && newAccount.Value != null)
            {
                var newMoney = Money.Create(income.Amount.Amount, currency);
                if (newMoney.IsSuccess)
                {
                    newAccount.Value.Credit(newMoney.Value!);
                    await _accountRepository.UpdateAsync(newAccount.Value);
                }
            }
        }
        else if (request.Amount.HasValue && request.Amount.Value != oldAmount)
        {
            // Handle amount change in the same account
            var account = await _accountRepository.GetByIdAsync(income.AccountId);
            if (account.IsSuccess && account.Value != null)
            {
                var difference = income.Amount.Amount - oldAmount;
                if (difference != 0)
                {
                    var diffMoney = Money.Create(Math.Abs(difference), currency);
                    if (diffMoney.IsSuccess)
                    {
                        if (difference > 0) account.Value.Credit(diffMoney.Value!);
                        else account.Value.Debit(diffMoney.Value!);
                        
                        await _accountRepository.UpdateAsync(account.Value);
                    }
                }
            }
        }

        var saveResult = await _incomeRepository.UpdateAsync(income);
        if (saveResult.IsFailure)
            return OperationResult.Failure<IncomeResponse>(saveResult.Error);

        return OperationResult.Success(await MapToResponseAsync(income));
    }

    public async Task<OperationResult> DeleteIncomeAsync(Guid userId, Guid incomeId)
    {
        var incomeResult = await _incomeRepository.GetByIdAsync(incomeId);
        if (incomeResult.IsFailure || incomeResult.Value == null)
            return OperationResult.Failure(incomeResult.Error ?? "Income not found");

        if (incomeResult.Value.UserId != userId)
            return OperationResult.Failure("Income not found");

        var income = incomeResult.Value;

        // Subtract the amount from the associated account (reverse credit)
        var account = await _accountRepository.GetByIdAsync(income.AccountId);
        if (account.IsSuccess && account.Value != null)
        {
            account.Value.Debit(income.Amount);
            await _accountRepository.UpdateAsync(account.Value);
        }

        return await _incomeRepository.DeleteAsync(incomeId);
    }

    public async Task<OperationResult<IncomeSummaryResponse>> GetMonthlySummaryAsync(Guid userId, int year, int month)
    {
        var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = startDate.AddMonths(1).AddTicks(-1);

        var dateRangeResult = DateRange.Create(startDate, endDate);
        if (dateRangeResult.IsFailure)
            return OperationResult.Failure<IncomeSummaryResponse>(dateRangeResult.Error);

        var incomesResult = await _incomeRepository.GetByUserAndDateRangeAsync(userId, dateRangeResult.Value!);
        if (incomesResult.IsFailure || incomesResult.Value == null)
            return OperationResult.Failure<IncomeSummaryResponse>(incomesResult.Error ?? "Incomes fail");

        var incomes = incomesResult.Value.ToList();
        
        if (!incomes.Any())
        {
            return OperationResult.Success(new IncomeSummaryResponse
            {
                TotalAmount = 0,
                Currency = "USD",
                IncomeCount = 0,
                AverageIncome = 0,
                TopSourceName = "N/A",
                TopSourceAmount = 0
            });
        }

        var currency = incomes.First().Amount.Currency;
        var totalAmount = incomes.Sum(i => i.Amount.Amount);

        var sourceGroups = incomes
            .GroupBy(i => i.IncomeSourceId)
            .OrderByDescending(g => g.Sum(i => i.Amount.Amount))
            .FirstOrDefault();

        var topSourceId = sourceGroups?.Key ?? Guid.Empty;
        var topSourceAmount = sourceGroups?.Sum(i => i.Amount.Amount) ?? 0;

        var sourceResult = topSourceId != Guid.Empty 
            ? await _incomeSourceRepository.GetByIdAsync(topSourceId) 
            : null;

        var summary = new IncomeSummaryResponse
        {
            TotalAmount = totalAmount,
            Currency = currency,
            IncomeCount = incomes.Count,
            AverageIncome = totalAmount / incomes.Count,
            TopSourceName = sourceResult?.Value?.Name ?? "Unknown",
            TopSourceAmount = topSourceAmount
        };

        return OperationResult.Success(summary);
    }

    public async Task<OperationResult<IEnumerable<IncomeSourceResponse>>> GetIncomeBySourceAsync(Guid userId, DateTime fromDate, DateTime toDate)
    {
       return await GetIncomeSourcesAsync(userId);
    }

    public async Task<OperationResult<IncomeSourceResponse>> CreateIncomeSourceAsync(Guid userId, CreateIncomeSourceRequest request)
    {
        var sourceResult = IncomeSource.Create(request.Name, userId, request.Description, request.Icon, request.Color);
        if (sourceResult.IsFailure || sourceResult.Value == null)
            return OperationResult.Failure<IncomeSourceResponse>(sourceResult.Error);
 
        var addResult = await _incomeSourceRepository.AddAsync(sourceResult.Value);
        if (addResult.IsFailure)
            return OperationResult.Failure<IncomeSourceResponse>(addResult.Error);
 
        return OperationResult.Success(MapToSourceResponse(sourceResult.Value));
    }

    public async Task<OperationResult<IEnumerable<IncomeSourceResponse>>> GetIncomeSourcesAsync(Guid userId)
    {
        var result = await _incomeSourceRepository.GetAllByUserAsync(userId);
        if (result.IsFailure || result.Value == null)
            return OperationResult.Failure<IEnumerable<IncomeSourceResponse>>(result.Error ?? "Sources not found");
 
        return OperationResult.Success(result.Value.Select(MapToSourceResponse));
    }

    public async Task<OperationResult<IncomeSourceResponse>> UpdateIncomeSourceAsync(Guid userId, Guid sourceId, CreateIncomeSourceRequest request)
    {
        var sourceResult = await _incomeSourceRepository.GetByIdAsync(sourceId);
        if (sourceResult.IsFailure || sourceResult.Value == null)
            return OperationResult.Failure<IncomeSourceResponse>(sourceResult.Error ?? "Income source not found");
 
        if (sourceResult.Value.UserId != userId)
            return OperationResult.Failure<IncomeSourceResponse>("Income source not found");

        sourceResult.Value.Update(request.Name, request.Description, request.Icon, request.Color);

        var updateResult = await _incomeSourceRepository.UpdateAsync(sourceResult.Value);
        if (updateResult.IsFailure)
            return OperationResult.Failure<IncomeSourceResponse>(updateResult.Error);

        return OperationResult.Success(MapToSourceResponse(sourceResult.Value));
    }

    public async Task<OperationResult> DeleteIncomeSourceAsync(Guid userId, Guid sourceId)
    {
        var sourceResult = await _incomeSourceRepository.GetByIdAsync(sourceId);
        if (sourceResult.IsFailure || sourceResult.Value == null)
            return OperationResult.Failure(sourceResult.Error ?? "Income source not found");
 
        if (sourceResult.Value.UserId != userId)
            return OperationResult.Failure("Income source not found");

        return await _incomeSourceRepository.DeleteAsync(sourceId);
    }

    public Task<OperationResult<IncomeResponse>> CreateRecurringIncomeAsync(Guid userId, CreateRecurringIncomeRequest request)
    {
        return Task.FromResult(OperationResult.Failure<IncomeResponse>("Recurring income implementation pending"));
    }

    public Task<OperationResult<IEnumerable<IncomeResponse>>> GetRecurringIncomesAsync(Guid userId)
    {
        return Task.FromResult(OperationResult.Success<IEnumerable<IncomeResponse>>(Enumerable.Empty<IncomeResponse>()));
    }

    public async Task<OperationResult<byte[]>> ExportToExcelAsync(Guid userId, DateTime fromDate, DateTime toDate)
    {
        var dateRangeResult = DateRange.Create(fromDate, toDate);
        if (dateRangeResult.IsFailure)
            return OperationResult.Failure<byte[]>(dateRangeResult.Error);

        var incomesResult = await _incomeRepository.GetByUserAndDateRangeAsync(userId, dateRangeResult.Value!);
        if (incomesResult.IsFailure || incomesResult.Value == null)
            return OperationResult.Failure<byte[]>(incomesResult.Error ?? "Incomes not found");

        var exportData = incomesResult.Value!.Select(i => new
        {
            Date = i.Date,
            i.Description,
            Amount = i.Amount.Amount,
            Currency = i.Amount.Currency,
            i.Notes
        });

        return await _exportService.ExportToExcelAsync(exportData, "Incomes");
    }

    private async Task<IncomeResponse> MapToResponseAsync(IncomeEntity income)
    {
        var source = await _incomeSourceRepository.GetByIdAsync(income.IncomeSourceId);
        var account = await _accountRepository.GetByIdAsync(income.AccountId);

        return new IncomeResponse
        {
            Id = income.Id,
            Amount = income.Amount.Amount,
            Currency = income.Amount.Currency,
            Description = income.Description,
            Date = income.Date,
            IncomeSourceId = income.IncomeSourceId,
            IncomeSourceName = (source.IsSuccess && source.Value != null) ? source.Value.Name : "Unknown",
            AccountId = income.AccountId,
            AccountName = (account.IsSuccess && account.Value != null) ? account.Value.Name : "Unknown",
            Notes = income.Notes,
            IsRecurring = income.IsRecurring,
            CreatedAt = income.CreatedAt
        };
    }

    private static IncomeSourceResponse MapToSourceResponse(IncomeSource source)
    {
        return new IncomeSourceResponse
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            Icon = source.Icon,
            Color = source.Color,
            IsActive = source.IsActive
        };
    }
}
