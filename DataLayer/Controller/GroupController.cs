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

    public static async ValueTask<Group?> AddGroupAsync(long groupId, CancellationToken ct = default)
    {
        try
        {
            await using var db = new ManagerContext();
            var exists = await db.Groups.FirstOrDefaultAsync(gp => gp.GroupId == groupId, cancellationToken: ct);
            if (exists is not null)
                return exists;

            var result = await db.Groups.AddAsync(new Group()
            {
                GroupId = groupId,
                BanOnCurse = false,
                WarnOnCurse = true,
                MaxWarns = 3,
                MuteTime = TimeSpan.FromHours(2),
                MuteOnCurse = true
            }, ct);

            await db.SaveChangesAsync(ct);
            return result.Entity;
        }
        catch (Exception e)
        {
            Log.Error(e, "AddGroupAsync");
            return null;
        }
    }

    public static async ValueTask<Group?> UpdateGroupAsync(Action<Group> update, long groupId, CancellationToken ct = default)
    {
        try
        {
            await using var db = new ManagerContext();
            var find = await db.Groups.FirstOrDefaultAsync(p => p.GroupId == groupId, ct);
            if (find is null)
                return null;

            update(find);

            await db.SaveChangesAsync(ct);
            return find;
        }
        catch (Exception e)
        {
            Log.Error(e, nameof(UpdateGroupAsync));
            return null;
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