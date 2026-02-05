namespace ManageMyMoney.Core.Application.DTOs.Accounts;

public record CreateAccountRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required string Type { get; init; }
    public decimal InitialBalance { get; init; } = 0;
    public string Currency { get; init; } = "USD";
    public string? Icon { get; init; }
    public string? Color { get; init; }
    public bool IncludeInTotal { get; init; } = true;
}

public record UpdateAccountRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Icon { get; init; }
    public string? Color { get; init; }
    public bool? IncludeInTotal { get; init; }
}

public record AccountResponse
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required string Type { get; init; }
    public decimal Balance { get; init; }
    public required string Currency { get; init; }
    public string? Icon { get; init; }
    public string? Color { get; init; }
    public bool IsActive { get; init; }
    public bool IncludeInTotal { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record AccountSummaryResponse
{
    public decimal TotalBalance { get; init; }
    public required string Currency { get; init; }
    public int ActiveAccountsCount { get; init; }
    public List<AccountBalanceItem> AccountBalances { get; init; } = new();
}

public record AccountBalanceItem
{
    public Guid AccountId { get; init; }
    public required string AccountName { get; init; }
    public required string AccountType { get; init; }
    public decimal Balance { get; init; }
    public required string Currency { get; init; }
}

public record TransferRequest
{
    public Guid FromAccountId { get; init; }
    public Guid ToAccountId { get; init; }
    public decimal Amount { get; init; }
    public string? Description { get; init; }
    public DateTime Date { get; init; }
}

public record TransferResponse
{
    public Guid Id { get; init; }
    public Guid FromAccountId { get; init; }
    public required string FromAccountName { get; init; }
    public Guid ToAccountId { get; init; }
    public required string ToAccountName { get; init; }
    public decimal Amount { get; init; }
    public required string Currency { get; init; }
    public string? Description { get; init; }
    public DateTime Date { get; init; }
}

public record CreatePaymentMethodRequest
{
    public required string Name { get; init; }
    public required string Type { get; init; }
    public string? LastFourDigits { get; init; }
    public string? Icon { get; init; }
    public Guid? AccountId { get; init; }
    public bool IsDefault { get; init; }
}

public record PaymentMethodResponse
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Type { get; init; }
    public string? LastFourDigits { get; init; }
    public string? Icon { get; init; }
    public Guid? AccountId { get; init; }
    public string? AccountName { get; init; }
    public bool IsDefault { get; init; }
    public bool IsActive { get; init; }
}

public record CreateCreditCardRequest
{
    public Guid AccountId { get; init; }
    public decimal CreditLimit { get; init; }
    public string Currency { get; init; } = "USD";
    public int StatementClosingDay { get; init; }
    public int PaymentDueDay { get; init; }
    public decimal? InterestRate { get; init; }
}

public record CreditCardResponse
{
    public Guid Id { get; init; }
    public Guid AccountId { get; init; }
    public required string AccountName { get; init; }
    public decimal CreditLimit { get; init; }
    public decimal CurrentBalance { get; init; }
    public decimal AvailableCredit { get; init; }
    public required string Currency { get; init; }
    public int StatementClosingDay { get; init; }
    public int PaymentDueDay { get; init; }
    public decimal? InterestRate { get; init; }
    public decimal UtilizationRate { get; init; }
}
