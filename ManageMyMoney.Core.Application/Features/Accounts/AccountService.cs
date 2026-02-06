using ManageMyMoney.Core.Application.DTOs.Accounts;
using ManageMyMoney.Core.Domain.Common;
using Microsoft.Extensions.Logging;

namespace ManageMyMoney.Core.Application.Features.Accounts;

public class AccountService : IAccountService
{
    private readonly ILogger<AccountService> _logger;

    public AccountService(ILogger<AccountService> logger)
    {
        _logger = logger;
    }

    public Task<OperationResult<AccountResponse>> CreateAccountAsync(Guid userId, CreateAccountRequest request)
    {
        _logger.LogWarning("AccountService.CreateAccountAsync not implemented yet");
        return Task.FromResult(OperationResult.Failure<AccountResponse>("Not implemented yet"));
    }

    public Task<OperationResult<AccountResponse>> GetAccountByIdAsync(Guid userId, Guid accountId)
    {
        _logger.LogWarning("AccountService.GetAccountByIdAsync not implemented yet");
        return Task.FromResult(OperationResult.Failure<AccountResponse>("Not implemented yet"));
    }

    public Task<OperationResult<IEnumerable<AccountResponse>>> GetAccountsAsync(Guid userId, bool activeOnly = true)
    {
        _logger.LogWarning("AccountService.GetAccountsAsync not implemented yet");
        return Task.FromResult(OperationResult.Failure<IEnumerable<AccountResponse>>("Not implemented yet"));
    }

    public Task<OperationResult<AccountResponse>> UpdateAccountAsync(Guid userId, Guid accountId, UpdateAccountRequest request)
    {
        _logger.LogWarning("AccountService.UpdateAccountAsync not implemented yet");
        return Task.FromResult(OperationResult.Failure<AccountResponse>("Not implemented yet"));
    }

    public Task<OperationResult> DeactivateAccountAsync(Guid userId, Guid accountId)
    {
        _logger.LogWarning("AccountService.DeactivateAccountAsync not implemented yet");
        return Task.FromResult(OperationResult.Failure("Not implemented yet"));
    }

    public Task<OperationResult<AccountSummaryResponse>> GetAccountsSummaryAsync(Guid userId)
    {
        _logger.LogWarning("AccountService.GetAccountsSummaryAsync not implemented yet");
        return Task.FromResult(OperationResult.Failure<AccountSummaryResponse>("Not implemented yet"));
    }

    public Task<OperationResult<TransferResponse>> TransferAsync(Guid userId, TransferRequest request)
    {
        _logger.LogWarning("AccountService.TransferAsync not implemented yet");
        return Task.FromResult(OperationResult.Failure<TransferResponse>("Not implemented yet"));
    }

    public Task<OperationResult<IEnumerable<TransferResponse>>> GetTransfersAsync(Guid userId, DateTime? fromDate, DateTime? toDate)
    {
        _logger.LogWarning("AccountService.GetTransfersAsync not implemented yet");
        return Task.FromResult(OperationResult.Failure<IEnumerable<TransferResponse>>("Not implemented yet"));
    }

    public Task<OperationResult<PaymentMethodResponse>> CreatePaymentMethodAsync(Guid userId, CreatePaymentMethodRequest request)
    {
        _logger.LogWarning("AccountService.CreatePaymentMethodAsync not implemented yet");
        return Task.FromResult(OperationResult.Failure<PaymentMethodResponse>("Not implemented yet"));
    }

    public Task<OperationResult<IEnumerable<PaymentMethodResponse>>> GetPaymentMethodsAsync(Guid userId)
    {
        _logger.LogWarning("AccountService.GetPaymentMethodsAsync not implemented yet");
        return Task.FromResult(OperationResult.Failure<IEnumerable<PaymentMethodResponse>>("Not implemented yet"));
    }

    public Task<OperationResult> SetDefaultPaymentMethodAsync(Guid userId, Guid paymentMethodId)
    {
        _logger.LogWarning("AccountService.SetDefaultPaymentMethodAsync not implemented yet");
        return Task.FromResult(OperationResult.Failure("Not implemented yet"));
    }

    public Task<OperationResult> DeletePaymentMethodAsync(Guid userId, Guid paymentMethodId)
    {
        _logger.LogWarning("AccountService.DeletePaymentMethodAsync not implemented yet");
        return Task.FromResult(OperationResult.Failure("Not implemented yet"));
    }

    public Task<OperationResult<CreditCardResponse>> CreateCreditCardAsync(Guid userId, CreateCreditCardRequest request)
    {
        _logger.LogWarning("AccountService.CreateCreditCardAsync not implemented yet");
        return Task.FromResult(OperationResult.Failure<CreditCardResponse>("Not implemented yet"));
    }

    public Task<OperationResult<IEnumerable<CreditCardResponse>>> GetCreditCardsAsync(Guid userId)
    {
        _logger.LogWarning("AccountService.GetCreditCardsAsync not implemented yet");
        return Task.FromResult(OperationResult.Failure<IEnumerable<CreditCardResponse>>("Not implemented yet"));
    }

    public Task<OperationResult<CreditCardResponse>> GetCreditCardByAccountAsync(Guid userId, Guid accountId)
    {
        _logger.LogWarning("AccountService.GetCreditCardByAccountAsync not implemented yet");
        return Task.FromResult(OperationResult.Failure<CreditCardResponse>("Not implemented yet"));
    }
}