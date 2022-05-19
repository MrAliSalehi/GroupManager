using GroupManager.DataLayer.Context;
using GroupManager.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace GroupManager.DataLayer.Controller;

public struct ForceJoinController
{
    public static async ValueTask<List<ForceJoinChannel>?> GetAllChannelsAsync(long groupId, CancellationToken ct = default)
    {
        try
        {
            await using var db = new ManagerContext();
            return await db.ForceJoinChannels.Where(p => p.GroupId == groupId).ToListAsync(ct);

        }
        catch (Exception e)
        {
            Log.Error(e, nameof(GetAllChannelsAsync));
            return null;
        }
    }

    public static async Task AddChannelAsync(long gpIdentifierInDb, string channelId, CancellationToken ct)
    {
        try
        {
            await using var db = new ManagerContext();
            var exists = await db.ForceJoinChannels.AnyAsync(p => p.GroupId == gpIdentifierInDb && p.ChannelId == channelId, ct);
            if (exists)
                return;

            await db.ForceJoinChannels.AddAsync(new ForceJoinChannel()
            {
                ChannelId = channelId,
                GroupId = gpIdentifierInDb,

            }, ct);
            await db.SaveChangesAsync(ct);

        }
        catch (Exception e)
        {
            Log.Error(e, nameof(AddChannelAsync));
        }
    }
    /// <summary>
    /// Remove ForceJoin Channel
    /// </summary>
    /// <param name="groupId">group identifier in db(PK)</param>
    /// <param name="channelId"></param>
    /// <param name="ct"></param>
    /// <returns>returns 0 on success,2 on exception,and 1 on not found</returns>
    public static async ValueTask<ushort> RemoveChannelAsync(long groupId, string channelId, CancellationToken ct)
    {
        try
        {
            await using var db = new ManagerContext();
            var ch = await db.ForceJoinChannels
                .FirstOrDefaultAsync(p => p.GroupId == groupId && p.ChannelId == channelId, ct);

            if (ch is null)
                return 1;
            db.ForceJoinChannels.Remove(ch);
            await db.SaveChangesAsync(ct);
            return 0;
        }
        catch (Exception e)
        {
            Log.Error(e, nameof(RemoveChannelAsync));
            return 2;
        }
    }

    /// <summary>
    /// remove all forcejoin channels entities related to groupId 
    /// </summary>
    /// <param name="groupId"></param>
    /// <param name="ct"></param>
    /// <returns>return 0 on success. 1 on nothing found and 2 on exception</returns>
    internal static async ValueTask<ushort> RemoveAllRelatedChannelsAsync(long groupId, CancellationToken ct = default)
    {
        try
        {
            await using var db = new ManagerContext();
            var findGroup = await db.ForceJoinChannels.Where(p => p.GroupId == groupId).ToListAsync(ct);
            if (!findGroup.Any())
                return 1;
            db.ForceJoinChannels.RemoveRange(findGroup);
            await db.SaveChangesAsync(ct);
            return 0;
        }
        catch (Exception e)
        {
            Log.Error(e, nameof(RemoveAllRelatedChannelsAsync));
            return 2;
        }
    }

}