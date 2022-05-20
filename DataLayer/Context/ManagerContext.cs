using GroupManager.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace GroupManager.DataLayer.Context;
#nullable disable

public class ManagerContext : DbContext
{
    public virtual DbSet<ForceJoinChannel> ForceJoinChannels { get; set; } = null!;
    public virtual DbSet<Group> Groups { get; set; } = null!;
    public virtual DbSet<User> Users { get; set; } = null!;
    public virtual DbSet<FloodSettings> FloodSettings { get; set; }

    private readonly ILoggerFactory _loggerFactory;

    public ManagerContext() : base() { }

    public ManagerContext(DbContextOptions<ManagerContext> options, ILoggerFactory loggerFactory) : base(options)
    {
        _loggerFactory = loggerFactory;
    }


    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseLoggerFactory(_loggerFactory);
        options.UseLazyLoadingProxies().UseSqlite("Data Source = ManagerDb.db");
        // Log.Information("DBContext Connection String:\n{0}", connString);
        base.OnConfiguring(options);

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FloodSettings>(entity =>
        {
            entity.HasOne(p => p.Group)
                .WithOne(x => x.FloodSetting)
                .HasForeignKey<FloodSettings>(x => x.GroupId);
        });
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasMany(p => p.Groups)
                .WithMany(x => x.Users);

        });
        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasMany(p => p.ForceJoinChannel)
                .WithOne(x => x.Group)
                .HasForeignKey(f => f.GroupId);

            entity.HasMany(p => p.Users)
                .WithMany(x => x.Groups);

            entity.Property(e => e.MaxWarns).HasDefaultValueSql("3");

            entity.Property(e => e.MuteTime).HasDefaultValueSql("'03:00:00'");

            entity.Property(e => e.WelcomeMessage).HasDefaultValueSql("'Welcome To Group!'");
        });

    }

}