using System.Linq.Expressions;
using GroupManager.DataLayer.Context;
using GroupManager.DataLayer.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace GroupManager.DataLayer.Controller;

public struct UserController
{
    /// <summary>
    /// update all of the users 
    /// </summary>
    /// <param name="update"></param>
    /// <param name="ct"></param>
    /// <returns>returns 0 on success and 1 on exception</returns>
    public static async ValueTask<ushort> UpdateAllUsersAsync(Action<User> update, CancellationToken ct = default)
    {
        try
        {
            await using var db = new ManagerContext();
            var users = await db.Users.ToListAsync(ct);
            foreach (var user in users)
            {
                update(user);
            }

            await db.SaveChangesAsync(ct);
            return 0;
        }
        catch (Exception e)
        {
            Log.Error(e, nameof(UpdateAllUsersAsync));
            return 1;
        }
    }

    public static async ValueTask<User> TryAddUserAsync(long userId, Group group, CancellationToken ct = default)
    {
        try
        {
            await using var db = new ManagerContext();
            var exists = await db.Users
                .Include(f => f.Groups)
                .Where(p => p.UserId == userId && p.Groups.Any(x => x.GroupId == group.GroupId))
                .FirstOrDefaultAsync(ct);
            if (exists is not null)
                return exists;
            var newUser = new User()
            {
                UserId = userId,
                GifLimits = group.GifLimits,
                PhotoLimits = group.PhotoLimits,
                VideoLimits = group.VideoLimits,
                StickerLimits = group.StickerLimits,
                Groups = new List<Group>()
            };
            var user = await db.Users.AddAsync(newUser, ct);
            await db.SaveChangesAsync(ct);

            newUser.Groups.Add(group);
            //db.Users.Attach(user.Entity);
            await db.SaveChangesAsync(ct);
            return user.Entity;
        }
        catch (Exception e)
        {
            Log.Error(e, nameof(TryAddUserAsync));
            return new User();
        }
    }

    public static async ValueTask<User?> GetUserByIdAsync<T>(long userId, Expression<Func<User, T>>? includeExpression, CancellationToken ct = default)
    {
        try
        {
            await using var db = new ManagerContext();
            if (includeExpression is null)
            {
                return await db.Users.FirstOrDefaultAsync(p => p.UserId == userId, ct);
            }
            else
            {
                return await db.Users.Where(p => p.UserId == userId).Include(includeExpression).FirstOrDefaultAsync(ct);
            }
        }
        catch (Exception e)
        {
            Log.Error(e, nameof(GetUserByIdAsync));
            return null;
        }
    }

    public static async ValueTask<User?> UpdateUserAsync(Action<User> update, long userId, CancellationToken ct = default)
    {
        try
        {
            await using var db = new ManagerContext();
            var getUser = await db.Users.FirstOrDefaultAsync(p => p.UserId == userId, ct);
            if (getUser is null)
                return null;
            update(getUser);

            await db.SaveChangesAsync(ct);
            return getUser;
        }
        catch (Exception e)
        {
            Log.Error(e, "UpdateUserAsync");
            return null;
        }
    }
}