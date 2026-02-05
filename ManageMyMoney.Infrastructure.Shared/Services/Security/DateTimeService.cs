using ManageMyMoney.Core.Application.Common.Interfaces;

namespace ManageMyMoney.Infrastructure.Shared.Services.Security;

public class DateTimeService : IDateTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Today => DateTime.UtcNow.Date;
    public DateOnly TodayDate => DateOnly.FromDateTime(DateTime.UtcNow);
}
