using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.Notifications;
using ManageMyMoney.Core.Domain.Interfaces;
using ManageMyMoney.Infrastructure.Persistence.Context;
using ManageMyMoney.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace ManageMyMoney.Infrastructure.Persistence.Repositories.Notifications;

public class AlertRepository : GenericRepository<Alert>, IAlertRepository
{
    public AlertRepository(ManageMyMoneyContext context) : base(context)
    {
    }

    public async Task<OperationResult<IEnumerable<Alert>>> GetByUserAsync(Guid userId, bool unacknowledgedOnly = true)
    {
        try
        {
            var query = DbSet.Where(a => a.UserId == userId);

            if (unacknowledgedOnly)
            {
                query = query.Where(a => !a.IsAcknowledged);
            }

            var alerts = await query
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return OperationResult.Success<IEnumerable<Alert>>(alerts);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<IEnumerable<Alert>>($"Error retrieving alerts: {ex.Message}");
        }
    }

    public async Task<OperationResult> DeleteAsync(Guid id)
    {
        try
        {
            var entity = await DbSet.FindAsync(id);
            if (entity == null)
                return OperationResult.Failure("Alert not found");

            DbSet.Remove(entity);
            await Context.SaveChangesAsync();
            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            return OperationResult.Failure($"Error deleting alert: {ex.Message}");
        }
    }
}
