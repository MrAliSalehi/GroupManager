using GroupManager.DataLayer.Context;
using GroupManager.DataLayer.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace GroupManager.DataLayer.Controller;

public struct UserController
{
    public static async ValueTask<User> TryAddUserAsync(long userId, CancellationToken ct = default)
    {
        try
        {
            await using var db = new ManagerContext();
            var exists = await db.Users.FirstOrDefaultAsync(p => p.UserId == userId, ct);
            if (exists is not null)
                return exists;

            var user = await db.Users.AddAsync(new User() { UserId = userId, }, ct);
            await db.SaveChangesAsync(ct);
            return user.Entity;
        }
        catch (Exception e)
        {
            Log.Error(e, "TryAddUserAsync");
            return new User();
        }
    }

    public static async ValueTask<User?> GetUserByIdAsync(long userId, CancellationToken ct = default)
    {
        try
        {
            await using var db = new ManagerContext();
            var user = await db.Users.FirstOrDefaultAsync(p => p.UserId == userId, ct);
            return user;
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