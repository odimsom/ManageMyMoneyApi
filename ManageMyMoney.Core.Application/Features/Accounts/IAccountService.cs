using ManageMyMoney.Core.Application.DTOs.Accounts;
using ManageMyMoney.Core.Domain.Common;

namespace ManageMyMoney.Core.Application.Features.Accounts;

public interface IAccountService
{
    // Accounts
    Task<OperationResult<AccountResponse>> CreateAccountAsync(Guid userId, CreateAccountRequest request);
    Task<OperationResult<AccountResponse>> GetAccountByIdAsync(Guid userId, Guid accountId);
    Task<OperationResult<IEnumerable<AccountResponse>>> GetAccountsAsync(Guid userId, bool activeOnly = true);
    Task<OperationResult<AccountResponse>> UpdateAccountAsync(Guid userId, Guid accountId, UpdateAccountRequest request);
    Task<OperationResult> DeactivateAccountAsync(Guid userId, Guid accountId);
    Task<OperationResult<AccountSummaryResponse>> GetAccountsSummaryAsync(Guid userId);

    // Transfers
    Task<OperationResult<TransferResponse>> TransferAsync(Guid userId, TransferRequest request);
    Task<OperationResult<IEnumerable<TransferResponse>>> GetTransfersAsync(Guid userId, DateTime? fromDate, DateTime? toDate);

    // Payment Methods
    Task<OperationResult<PaymentMethodResponse>> CreatePaymentMethodAsync(Guid userId, CreatePaymentMethodRequest request);
    Task<OperationResult<IEnumerable<PaymentMethodResponse>>> GetPaymentMethodsAsync(Guid userId);
    Task<OperationResult> SetDefaultPaymentMethodAsync(Guid userId, Guid paymentMethodId);
    Task<OperationResult> DeletePaymentMethodAsync(Guid userId, Guid paymentMethodId);

    // Credit Cards
    Task<OperationResult<CreditCardResponse>> CreateCreditCardAsync(Guid userId, CreateCreditCardRequest request);
    Task<OperationResult<IEnumerable<CreditCardResponse>>> GetCreditCardsAsync(Guid userId);
    Task<OperationResult<CreditCardResponse>> GetCreditCardByAccountAsync(Guid userId, Guid accountId);
}
