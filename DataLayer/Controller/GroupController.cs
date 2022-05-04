using GroupManager.Application.Commands;
using GroupManager.DataLayer.Context;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Group = GroupManager.DataLayer.Models.Group;

namespace GroupManager.DataLayer.Controller;

public struct GroupController
{
    public static async ValueTask<IReadOnlyList<Group>> GetAllGroupsAsync(CancellationToken ct = default)
    {
        try
        {
            await using var db = new ManagerContext();
            return await db.Groups.ToListAsync(ct);
        }
        catch (Exception e)
        {
            Log.Error(e, "GetAllGroupsAsync");
            return Array.Empty<Group>();
        }
    }

    public static async ValueTask<Group?> GetGroupByIdAsync(long groupId, CancellationToken ct = default)
    {
        try
        {
            await using var db = new ManagerContext();
            return await db.Groups.FirstOrDefaultAsync(p => p.GroupId == groupId, ct);

        }
        catch (Exception e)
        {
            Log.Error(e, "GetGroupByIdAsync");
            return null;
        }
    }

    public static async Task AddGroupAsync(long groupId, CancellationToken ct = default)
    {
        try
        {
            await using var db = new ManagerContext();
            var exists = await db.Groups.AnyAsync(gp => gp.GroupId == groupId, cancellationToken: ct);
            if (exists)
                return;
            await db.Groups.AddAsync(new Group() { GroupId = groupId }, ct);
            await db.SaveChangesAsync(ct);
        }
        catch (Exception e)
        {
            Log.Error(e, "AddGroupAsync");
        }
    }

    /// <summary>
    /// Remove Group From Db And Will Not Be Managed
    /// </summary>
    /// <param name="groupId"></param>
    /// <param name="ct"></param>
    /// <returns>return 0 means success,1 means group not found and 2 is error in db</returns>
    public static async ValueTask<byte> RemoveGroupAsync(long groupId, CancellationToken ct = default)
    {
        try
        {
            await using var db = new ManagerContext();
            var findGroup = await db.Groups.FirstOrDefaultAsync(gp => gp.GroupId == groupId, cancellationToken: ct);
            if (findGroup is null)
                return 1;

            db.Groups.Remove(findGroup);
            await db.SaveChangesAsync(ct);
            return 0;
        }
        catch (Exception e)
        {
            Log.Error(e, "RemoveGroupAsync");
            return 2;
        }
    }
}