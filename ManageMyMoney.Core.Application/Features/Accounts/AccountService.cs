using ManageMyMoney.Core.Application.DTOs.Accounts;
using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.Accounts;
using ManageMyMoney.Core.Domain.Enums;
using ManageMyMoney.Core.Domain.Interfaces;
using ManageMyMoney.Core.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace ManageMyMoney.Core.Application.Features.Accounts;

public class AccountService : IAccountService
{
    private readonly ILogger<AccountService> _logger;
    private readonly IAccountRepository _accountRepository;

    public AccountService(ILogger<AccountService> logger, IAccountRepository accountRepository)
    {
        _logger = logger;
        _accountRepository = accountRepository;
    }

    public async Task<OperationResult<AccountResponse>> CreateAccountAsync(Guid userId, CreateAccountRequest request)
    {
        try
        {
            _logger.LogInformation("Creating account {AccountName} for user {UserId}", request.Name, userId);

            // Convertir string a enum
            if (!Enum.TryParse<AccountType>(request.Type, true, out var accountType))
                return OperationResult.Failure<AccountResponse>("Invalid account type");

            // Crear cuenta
            var createResult = Account.Create(
                request.Name,
                accountType,
                userId,
                request.InitialBalance,
                request.Currency,
                request.Description,
                request.Icon,
                request.Color,
                request.IncludeInTotal
            );

            if (!createResult.IsSuccess)
                return OperationResult.Failure<AccountResponse>(createResult.Error);

            // Guardar en el repositorio
            var addResult = await _accountRepository.AddAsync(createResult.Value);
            if (!addResult.IsSuccess)
                return OperationResult.Failure<AccountResponse>(addResult.Error);

            var response = MapToResponse(createResult.Value);
            _logger.LogInformation("Account {AccountId} created successfully for user {UserId}", createResult.Value.Id, userId);
            
            return OperationResult.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating account for user {UserId}", userId);
            return OperationResult.Failure<AccountResponse>("An error occurred while creating the account");
        }
    }

    public async Task<OperationResult<AccountResponse>> GetAccountByIdAsync(Guid userId, Guid accountId)
    {
        try
        {
            _logger.LogInformation("Getting account {AccountId} for user {UserId}", accountId, userId);

            var result = await _accountRepository.GetByIdAsync(accountId);
            if (!result.IsSuccess)
                return OperationResult.Failure<AccountResponse>(result.Error);

            if (result.Value.UserId != userId)
                return OperationResult.Failure<AccountResponse>("Account not found");

            var response = MapToResponse(result.Value);
            return OperationResult.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting account {AccountId} for user {UserId}", accountId, userId);
            return OperationResult.Failure<AccountResponse>("An error occurred while retrieving the account");
        }
    }

    public async Task<OperationResult<IEnumerable<AccountResponse>>> GetAccountsAsync(Guid userId, bool activeOnly = true)
    {
        try
        {
            _logger.LogInformation("Getting accounts for user {UserId}, activeOnly: {ActiveOnly}", userId, activeOnly);

            var result = await _accountRepository.GetActiveByUserAsync(userId);
            if (!result.IsSuccess)
                return OperationResult.Failure<IEnumerable<AccountResponse>>(result.Error);

            var responses = result.Value.Select(MapToResponse);
            return OperationResult.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting accounts for user {UserId}", userId);
            return OperationResult.Failure<IEnumerable<AccountResponse>>("An error occurred while retrieving accounts");
        }
    }

    public async Task<OperationResult<AccountResponse>> UpdateAccountAsync(Guid userId, Guid accountId, UpdateAccountRequest request)
    {
        try
        {
            _logger.LogInformation("Updating account {AccountId} for user {UserId}", accountId, userId);

            // Obtener cuenta existente
            var getAccountResult = await _accountRepository.GetByIdAsync(accountId);
            if (!getAccountResult.IsSuccess)
                return OperationResult.Failure<AccountResponse>(getAccountResult.Error);

            var account = getAccountResult.Value;
            if (account.UserId != userId)
                return OperationResult.Failure<AccountResponse>("Account not found");

            // Actualizar detalles
            var updateResult = account.UpdateDetails(
                request.Name,
                request.Description,
                request.Icon,
                request.Color,
                request.IncludeInTotal
            );

            if (!updateResult.IsSuccess)
                return OperationResult.Failure<AccountResponse>(updateResult.Error);

            // Guardar cambios
            var saveResult = await _accountRepository.UpdateAsync(account);
            if (!saveResult.IsSuccess)
                return OperationResult.Failure<AccountResponse>(saveResult.Error);

            var response = MapToResponse(account);
            _logger.LogInformation("Account {AccountId} updated successfully", accountId);
            
            return OperationResult.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating account {AccountId} for user {UserId}", accountId, userId);
            return OperationResult.Failure<AccountResponse>("An error occurred while updating the account");
        }
    }

    public async Task<OperationResult> DeactivateAccountAsync(Guid userId, Guid accountId)
    {
        try
        {
            _logger.LogInformation("Deactivating account {AccountId} for user {UserId}", accountId, userId);

            // Obtener cuenta existente
            var getAccountResult = await _accountRepository.GetByIdAsync(accountId);
            if (!getAccountResult.IsSuccess)
                return OperationResult.Failure(getAccountResult.Error);

            var account = getAccountResult.Value;
            if (account.UserId != userId)
                return OperationResult.Failure("Account not found");

            // Desactivar cuenta
            var deactivateResult = account.Deactivate();
            if (!deactivateResult.IsSuccess)
                return OperationResult.Failure(deactivateResult.Error);

            // Guardar cambios
            var updateResult = await _accountRepository.UpdateAsync(account);
            if (!updateResult.IsSuccess)
                return OperationResult.Failure(updateResult.Error);

            _logger.LogInformation("Account {AccountId} deactivated successfully", accountId);
            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating account {AccountId} for user {UserId}", accountId, userId);
            return OperationResult.Failure("An error occurred while deactivating the account");
        }
    }

    public async Task<OperationResult<AccountSummaryResponse>> GetAccountsSummaryAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("Getting accounts summary for user {UserId}", userId);

            var result = await _accountRepository.GetActiveByUserAsync(userId);
            if (!result.IsSuccess)
                return OperationResult.Failure<AccountSummaryResponse>(result.Error);

            var accounts = result.Value.ToList();
            var accountBalances = accounts
                .Where(a => a.IncludeInTotal)
                .Select(a => new AccountBalanceItem
                {
                    AccountId = a.Id,
                    AccountName = a.Name,
                    AccountType = a.Type.ToString(),
                    Balance = a.Balance.Amount,
                    Currency = a.Balance.Currency
                })
                .ToList();

            var totalBalance = accountBalances
                .Where(ab => ab.Currency == "USD") // TODO: Implement currency conversion
                .Sum(ab => ab.Balance);

            var response = new AccountSummaryResponse
            {
                TotalBalance = totalBalance,
                Currency = "USD", // TODO: Use user's default currency
                ActiveAccountsCount = accounts.Count,
                AccountBalances = accountBalances
            };

            return OperationResult.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting accounts summary for user {UserId}", userId);
            return OperationResult.Failure<AccountSummaryResponse>("An error occurred while retrieving the accounts summary");
        }
    }

    public async Task<OperationResult<TransferResponse>> TransferAsync(Guid userId, TransferRequest request)
    {
        try
        {
            _logger.LogInformation("Processing transfer from account {FromAccountId} to {ToAccountId} for user {UserId}", 
                request.FromAccountId, request.ToAccountId, userId);

            if (request.Amount <= 0)
                return OperationResult.Failure<TransferResponse>("Transfer amount must be greater than zero");

            if (request.FromAccountId == request.ToAccountId)
                return OperationResult.Failure<TransferResponse>("Cannot transfer to the same account");

            // Obtener cuenta origen
            var fromAccountResult = await _accountRepository.GetByIdAsync(request.FromAccountId);
            if (!fromAccountResult.IsSuccess || fromAccountResult.Value.UserId != userId)
                return OperationResult.Failure<TransferResponse>("Source account not found");

            // Obtener cuenta destino
            var toAccountResult = await _accountRepository.GetByIdAsync(request.ToAccountId);
            if (!toAccountResult.IsSuccess || toAccountResult.Value.UserId != userId)
                return OperationResult.Failure<TransferResponse>("Destination account not found");

            var fromAccount = fromAccountResult.Value;
            var toAccount = toAccountResult.Value;

            // Crear objeto Money para el monto
            var amountResult = Money.Create(request.Amount, fromAccount.Currency);
            if (!amountResult.IsSuccess)
                return OperationResult.Failure<TransferResponse>(amountResult.Error);

            // Validar que las monedas coincidan
            if (fromAccount.Currency != toAccount.Currency)
                return OperationResult.Failure<TransferResponse>("Currency mismatch between accounts");

            // Validar que la cuenta origen tenga fondos suficientes
            if (fromAccount.Balance.Amount < request.Amount)
                return OperationResult.Failure<TransferResponse>("Insufficient funds in source account");

            // Realizar la transferencia
            var debitResult = fromAccount.Debit(amountResult.Value);
            if (!debitResult.IsSuccess)
                return OperationResult.Failure<TransferResponse>(debitResult.Error);

            var creditResult = toAccount.Credit(amountResult.Value);
            if (!creditResult.IsSuccess)
            {
                // Revertir el débito si el crédito falla
                fromAccount.Credit(amountResult.Value);
                return OperationResult.Failure<TransferResponse>(creditResult.Error);
            }

            // Actualizar ambas cuentas
            var updateFromResult = await _accountRepository.UpdateAsync(fromAccount);
            if (!updateFromResult.IsSuccess)
                return OperationResult.Failure<TransferResponse>("Failed to update source account");

            var updateToResult = await _accountRepository.UpdateAsync(toAccount);
            if (!updateToResult.IsSuccess)
            {
                // TODO: Implementar transacción para rollback completo
                return OperationResult.Failure<TransferResponse>("Failed to update destination account");
            }

            // TODO: Crear registro de transferencia en la base de datos
            var response = new TransferResponse
            {
                Id = Guid.NewGuid(),
                FromAccountId = request.FromAccountId,
                FromAccountName = fromAccount.Name,
                ToAccountId = request.ToAccountId,
                ToAccountName = toAccount.Name,
                Amount = request.Amount,
                Currency = fromAccount.Currency,
                Description = request.Description,
                Date = request.Date
            };

            _logger.LogInformation("Transfer completed successfully from {FromAccountId} to {ToAccountId}", 
                request.FromAccountId, request.ToAccountId);
            
            return OperationResult.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing transfer for user {UserId}", userId);
            return OperationResult.Failure<TransferResponse>("An error occurred while processing the transfer");
        }
    }

    public async Task<OperationResult<IEnumerable<TransferResponse>>> GetTransfersAsync(Guid userId, DateTime? fromDate, DateTime? toDate)
    {
        try
        {
            _logger.LogInformation("Getting transfers for user {UserId} from {FromDate} to {ToDate}", userId, fromDate, toDate);
            
            // TODO: Implementar cuando se cree el repositorio de AccountTransaction
            var emptyList = Enumerable.Empty<TransferResponse>();
            return OperationResult.Success(emptyList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transfers for user {UserId}", userId);
            return OperationResult.Failure<IEnumerable<TransferResponse>>("An error occurred while retrieving transfers");
        }
    }

    // Payment Methods (placeholders until PaymentMethod repository is implemented)
    public async Task<OperationResult<PaymentMethodResponse>> CreatePaymentMethodAsync(Guid userId, CreatePaymentMethodRequest request)
    {
        try
        {
            _logger.LogInformation("Creating payment method for user {UserId}", userId);
            
            // TODO: Implementar cuando se cree el repositorio de PaymentMethod
            return OperationResult.Failure<PaymentMethodResponse>("Payment method functionality not yet available");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment method for user {UserId}", userId);
            return OperationResult.Failure<PaymentMethodResponse>("An error occurred while creating the payment method");
        }
    }

    public async Task<OperationResult<IEnumerable<PaymentMethodResponse>>> GetPaymentMethodsAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("Getting payment methods for user {UserId}", userId);
            
            // TODO: Implementar cuando se cree el repositorio de PaymentMethod
            var emptyList = Enumerable.Empty<PaymentMethodResponse>();
            return OperationResult.Success(emptyList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment methods for user {UserId}", userId);
            return OperationResult.Failure<IEnumerable<PaymentMethodResponse>>("An error occurred while retrieving payment methods");
        }
    }

    public async Task<OperationResult> SetDefaultPaymentMethodAsync(Guid userId, Guid paymentMethodId)
    {
        try
        {
            _logger.LogInformation("Setting default payment method {PaymentMethodId} for user {UserId}", paymentMethodId, userId);
            
            // TODO: Implementar cuando se cree el repositorio de PaymentMethod
            return OperationResult.Failure("Payment method functionality not yet available");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default payment method for user {UserId}", userId);
            return OperationResult.Failure("An error occurred while setting the default payment method");
        }
    }

    public async Task<OperationResult> DeletePaymentMethodAsync(Guid userId, Guid paymentMethodId)
    {
        try
        {
            _logger.LogInformation("Deleting payment method {PaymentMethodId} for user {UserId}", paymentMethodId, userId);
            
            // TODO: Implementar cuando se cree el repositorio de PaymentMethod
            return OperationResult.Failure("Payment method functionality not yet available");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting payment method for user {UserId}", userId);
            return OperationResult.Failure("An error occurred while deleting the payment method");
        }
    }

    // Credit Cards (placeholders until CreditCard repository is implemented)
    public async Task<OperationResult<CreditCardResponse>> CreateCreditCardAsync(Guid userId, CreateCreditCardRequest request)
    {
        try
        {
            _logger.LogInformation("Creating credit card for account {AccountId}, user {UserId}", request.AccountId, userId);
            
            // TODO: Implementar cuando se cree el repositorio de CreditCard
            return OperationResult.Failure<CreditCardResponse>("Credit card functionality not yet available");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating credit card for user {UserId}", userId);
            return OperationResult.Failure<CreditCardResponse>("An error occurred while creating the credit card");
        }
    }

    public async Task<OperationResult<IEnumerable<CreditCardResponse>>> GetCreditCardsAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("Getting credit cards for user {UserId}", userId);
            
            // TODO: Implementar cuando se cree el repositorio de CreditCard
            var emptyList = Enumerable.Empty<CreditCardResponse>();
            return OperationResult.Success(emptyList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting credit cards for user {UserId}", userId);
            return OperationResult.Failure<IEnumerable<CreditCardResponse>>("An error occurred while retrieving credit cards");
        }
    }

    public async Task<OperationResult<CreditCardResponse>> GetCreditCardByAccountAsync(Guid userId, Guid accountId)
    {
        try
        {
            _logger.LogInformation("Getting credit card for account {AccountId}, user {UserId}", accountId, userId);
            
            // TODO: Implementar cuando se cree el repositorio de CreditCard
            return OperationResult.Failure<CreditCardResponse>("Credit card functionality not yet available");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting credit card for account {AccountId}, user {UserId}", accountId, userId);
            return OperationResult.Failure<CreditCardResponse>("An error occurred while retrieving the credit card");
        }
    }

    // Helper methods
    private static AccountResponse MapToResponse(Account account)
    {
        return new AccountResponse
        {
            Id = account.Id,
            Name = account.Name,
            Description = account.Description,
            Type = account.Type.ToString(),
            Balance = account.Balance.Amount,
            Currency = account.Currency,
            Icon = account.Icon,
            Color = account.Color,
            IsActive = account.IsActive,
            IncludeInTotal = account.IncludeInTotal,
            CreatedAt = account.CreatedAt
        };
    }
}