using GroupManager.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace GroupManager.DataLayer.Context;
#nullable disable

public class ManagerContext : DbContext
{
    public virtual DbSet<ForceJoinChannel> ForceJoinChannels { get; set; } = null!;
    public virtual DbSet<Group> Groups { get; set; } = null!;
    public virtual DbSet<User> Users { get; set; } = null!;

    private readonly ILoggerFactory _loggerFactory;
    private static string DbPath => "ManagerDb.db";

    public ManagerContext() : base() { }

    public ManagerContext(DbContextOptions<ManagerContext> options, ILoggerFactory loggerFactory) : base(options)
    {
        _loggerFactory = loggerFactory;
    }


    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {

        options.UseLoggerFactory(_loggerFactory);
        options.UseLazyLoadingProxies().UseSqlite($"Data Source={DbPath}");
        base.OnConfiguring(options);

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasMany(p => p.ForceJoinChannel)
                .WithOne(x => x.Group)
                .HasForeignKey(f => f.GroupId);


            entity.Property(e => e.MaxWarns).HasDefaultValueSql("3");

            entity.Property(e => e.MuteTime).HasDefaultValueSql("'03:00:00'");

            entity.Property(e => e.WelcomeMessage).HasDefaultValueSql("'Welcome To Group!'");
        });

    }

}