using GroupManager.DataLayer.Context;
using GroupManager.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace GroupManager.DataLayer.Controller;

public struct FloodController
{
    internal static async ValueTask<List<FloodSettings>?> GetAllFloodEnabledGroupsAsync(CancellationToken ct = default)
    {
        try
        {
            await using var db = new ManagerContext();
            var list = await db.FloodSettings
                .Where(p => p.Enabled == true)
                .Include(p => p.Group)
                .ToListAsync(ct);
            return list;
        }
        catch (Exception e)
        {
            Log.Error(e, nameof(GetAllFloodEnabledGroupsAsync));
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="groupId">This Is Db Identifier Not Telegram id</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    internal static async ValueTask<FloodSettings?> GetFloodSettingAsync(long groupId, CancellationToken ct = default)
    {
        try
        {
            await using var db = new ManagerContext();
            return await db.FloodSettings.FirstOrDefaultAsync(p => p.GroupId == groupId, ct);
        }
        catch (Exception e)
        {
            Log.Error(e, nameof(GetFloodSettingAsync));
            return null;
        }
    }

    internal static async ValueTask<FloodSettings?> AddFloodSettingAsync(long groupId, FloodSettings settings, CancellationToken ct = default)
    {
        try
        {
            await using var db = new ManagerContext();
            var exists = await db.FloodSettings.FirstOrDefaultAsync(p => p.GroupId == groupId, ct);
            if (exists is not null)
                return exists;
            settings.GroupId = groupId;
            var result = await db.FloodSettings.AddAsync(settings, ct);
            await db.SaveChangesAsync(ct);
            return result.Entity;
        }
        catch (Exception e)
        {
            Log.Error(e, nameof(AddFloodSettingAsync));
            return null;
        }
    }

    internal static async Task UpdateSettingsAsync(Action<FloodSettings> settings, long groupId, CancellationToken ct = default)
    {
        try
        {
            await using var db = new ManagerContext();
            var find = await db.FloodSettings.FirstOrDefaultAsync(p => p.GroupId == groupId, ct);
            if (find is null)
                return;
            settings(find);
            await db.SaveChangesAsync(ct);

        }
        catch (Exception e)
        {
            Log.Error(e, nameof(UpdateSettingsAsync));
        }
    }
    /// <summary>
    /// remove setting entity for given group id
    /// </summary>
    /// <param name="groupId"></param>
    /// <param name="ct"></param>
    /// <returns>returns 0 on success , 1 on nothing found and 2 on exception</returns>
    public static async ValueTask<ushort> RemoveSettingsAsync(long groupId, CancellationToken ct = default)
    {
        try
        {
            await using var db = new ManagerContext();
            var find = await db.FloodSettings.SingleOrDefaultAsync(p => p.GroupId == groupId, ct);
            if (find is null)
                return 1;
            db.FloodSettings.Remove(find);
            await db.SaveChangesAsync(ct);
            return 0;

        }
        catch (Exception e)
        {
            Log.Error(e, nameof(RemoveSettingsAsync));
            return 2;
        }
    }
}