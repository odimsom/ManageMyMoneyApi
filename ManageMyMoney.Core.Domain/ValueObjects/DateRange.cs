using ManageMyMoney.Core.Domain.Common;

namespace ManageMyMoney.Core.Domain.ValueObjects;

public sealed class DateRange : IEquatable<DateRange>
{
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }

    private DateRange(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }

    public static OperationResult<DateRange> Create(DateTime startDate, DateTime endDate)
    {
        if (endDate < startDate)
            return OperationResult.Failure<DateRange>("End date cannot be before start date");

        return OperationResult.Success(new DateRange(startDate, endDate));
    }

    public static DateRange CurrentMonth()
    {
        var now = DateTime.UtcNow;
        var start = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddMonths(1).AddTicks(-1);
        return new DateRange(start, end);
    }

    public bool Contains(DateTime date) => date >= StartDate && date <= EndDate;

    public int TotalDays => (int)(EndDate - StartDate).TotalDays + 1;

    public bool Equals(DateRange? other)
    {
        if (other is null) return false;
        return StartDate == other.StartDate && EndDate == other.EndDate;
    }

    public override bool Equals(object? obj) => Equals(obj as DateRange);

    public override int GetHashCode() => HashCode.Combine(StartDate, EndDate);
}
