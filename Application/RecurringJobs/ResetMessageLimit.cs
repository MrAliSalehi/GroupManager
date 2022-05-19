using GroupManager.DataLayer.Controller;

namespace GroupManager.Application.RecurringJobs;

public class ResetMessageLimit
{
    public async Task ResetUserLimitAsync(long userId, CancellationToken ct = default)
    {
        await UserController.UpdateUserAsync(p => p.MessageCount = 0, userId, ct);
    }
}