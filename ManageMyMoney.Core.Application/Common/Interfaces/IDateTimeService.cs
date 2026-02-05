namespace ManageMyMoney.Core.Application.Common.Interfaces;

public interface IDateTimeService
{
    DateTime UtcNow { get; }
    DateTime Today { get; }
    DateOnly TodayDate { get; }
}
