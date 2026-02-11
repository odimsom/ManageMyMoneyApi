using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.Notifications;
using ManageMyMoney.Core.Domain.Interfaces;
using ManageMyMoney.Infrastructure.Persistence.Context;
using ManageMyMoney.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace ManageMyMoney.Infrastructure.Persistence.Repositories.Notifications;

public class ReminderRepository : GenericRepository<Reminder>, IReminderRepository
{
    public ReminderRepository(ManageMyMoneyContext context) : base(context)
    {
    }

    public async Task<OperationResult<IEnumerable<Reminder>>> GetByUserAsync(Guid userId, bool pendingOnly = true)
    {
        try
        {
            var query = DbSet.Where(r => r.UserId == userId);

            if (pendingOnly)
            {
                query = query.Where(r => !r.IsCompleted);
            }

            var reminders = await query
                .OrderBy(r => r.DueDate)
                .ToListAsync();

            return OperationResult.Success<IEnumerable<Reminder>>(reminders);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<IEnumerable<Reminder>>($"Error retrieving reminders: {ex.Message}");
        }
    }

    public async Task<OperationResult<IEnumerable<Reminder>>> GetDueRemindersAsync()
    {
        try
        {
            var reminders = await DbSet
                .Where(r => !r.IsCompleted && !r.IsSent && r.DueDate <= DateTime.UtcNow)
                .ToListAsync();

            return OperationResult.Success<IEnumerable<Reminder>>(reminders);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<IEnumerable<Reminder>>($"Error retrieving due reminders: {ex.Message}");
        }
    }

    public async Task<OperationResult> DeleteAsync(Guid id)
    {
        try
        {
            var entity = await DbSet.FindAsync(id);
            if (entity == null)
                return OperationResult.Failure("Reminder not found");

            DbSet.Remove(entity);
            await Context.SaveChangesAsync();
            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            return OperationResult.Failure($"Error deleting reminder: {ex.Message}");
        }
    }
}
