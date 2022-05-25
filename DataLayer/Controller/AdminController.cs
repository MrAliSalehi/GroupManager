using GroupManager.DataLayer.Context;
using GroupManager.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace GroupManager.DataLayer.Controller;

public struct AdminController
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="adminId"></param>
    /// <param name="groupId"></param>
    /// <param name="ct"></param>
    /// <returns>returns 0 if exists,1 on success and 2 on fail</returns>
    public static async ValueTask<ushort> CreateAdminIfNotExistsAsync(long adminId, long groupId, CancellationToken ct = default)
    {
        try
        {
            await using var db = new ManagerContext();
            var exists = await db.Admins.AnyAsync(p => p.UserId == adminId && p.GroupId == groupId, cancellationToken: ct);
            if (exists)
                return 0;
            await db.Admins.AddAsync(new Admin { GroupId = groupId, UserId = adminId }, ct);
            await db.SaveChangesAsync(ct);
            return 1;
        }
        catch (Exception e)
        {
            Log.Error(e, nameof(CreateAdminIfNotExistsAsync));
            return 2;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="groupId"></param>
    /// <param name="ct"></param>
    /// <returns>return 0 on not found,1 on success and 2 on exception</returns>
    public static async ValueTask<ushort> RemoveAdminAsync(long userId, long groupId, CancellationToken ct = default)
    {
        try
        {
            await using var db = new ManagerContext();
            var admin = await db.Admins.FirstOrDefaultAsync(p => p.GroupId == groupId && p.UserId == userId, ct).ConfigureAwait(false);
            if (admin is null)
                return 0;
            db.Admins.Remove(admin);
            await db.SaveChangesAsync(ct).ConfigureAwait(false);
            return 1;
        }
        catch (Exception e)
        {
            Log.Error(e, nameof(RemoveAdminAsync));
            return 2;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="groupId">group identifier in db</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async ValueTask<List<Admin>?> GetAllAdminsAsync(long groupId, CancellationToken ct = default)
    {
        try
        {
            await using var db = new ManagerContext();
            var all = await db.Admins
                .Where(p => p.GroupId == groupId)
                .ToListAsync(ct)
                .ConfigureAwait(false);
            return all;
        }
        catch (Exception e)
        {
            Log.Error(e, nameof(GetAllAdminsAsync));
            return null;
        }
    }
}