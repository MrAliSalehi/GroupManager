using GroupManager.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace GroupManager.DataLayer.Context;
#nullable disable

public class ManagerContext : DbContext
{
    public DbSet<Group> Groups { get; set; }
    public DbSet<User> Users { get; set; }

    private readonly ILoggerFactory _loggerFactory;
    private static string DbPath => "ManagerDb.db";

    public ManagerContext() : base() { }

    public ManagerContext(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }


    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseLoggerFactory(_loggerFactory);
        options.UseSqlite($"Data Source={DbPath}");
        base.OnConfiguring(options);

    }

}