using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.ValueObjects;

namespace ManageMyMoney.Core.Domain.Entities.System;

public class TaxRate
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Percentage Rate { get; private set; } = Percentage.Zero;
    public string? CountryCode { get; private set; }
    public string? CategoryCode { get; private set; }
    public DateTime EffectiveFrom { get; private set; }
    public DateTime? EffectiveTo { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private TaxRate() { }

    public static OperationResult<TaxRate> Create(
        string name,
        decimal ratePercentage,
        DateTime effectiveFrom,
        string? description = null,
        string? countryCode = null,
        string? categoryCode = null,
        DateTime? effectiveTo = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return OperationResult.Failure<TaxRate>("Tax rate name is required");

        var percentageResult = Percentage.Create(ratePercentage);
        if (percentageResult.IsFailure)
            return OperationResult.Failure<TaxRate>(percentageResult.Error);

        if (effectiveTo.HasValue && effectiveTo < effectiveFrom)
            return OperationResult.Failure<TaxRate>("Effective end date cannot be before start date");

        var taxRate = new TaxRate
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description?.Trim(),
            Rate = percentageResult.Value!,
            CountryCode = countryCode?.ToUpperInvariant(),
            CategoryCode = categoryCode,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        return OperationResult.Success(taxRate);
    }

    public Money CalculateTax(Money amount) => Rate.ApplyTo(amount);

    public bool IsEffectiveOn(DateTime date) =>
        date >= EffectiveFrom && (!EffectiveTo.HasValue || date <= EffectiveTo.Value);

    public void Deactivate() => IsActive = false;
}
