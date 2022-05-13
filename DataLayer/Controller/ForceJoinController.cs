using GroupManager.DataLayer.Context;
using GroupManager.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace GroupManager.DataLayer.Controller;

public struct ForceJoinController
{
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

    public static async Task RemoveChannelAsync(long groupId, string channelId, CancellationToken ct)
    {
        try
        {
            await using var db = new ManagerContext();
            var ch = await db.ForceJoinChannels.FirstOrDefaultAsync(p => p.Group.GroupId == groupId && p.ChannelId == channelId, ct);
            if (ch is null)
                return;
            db.ForceJoinChannels.Remove(ch);
            await db.SaveChangesAsync(ct);
        }
        catch (Exception e)
        {
            Log.Error(e, nameof(RemoveChannelAsync));
        }
    }

}