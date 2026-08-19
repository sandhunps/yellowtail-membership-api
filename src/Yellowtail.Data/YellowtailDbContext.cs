using Microsoft.EntityFrameworkCore;
using Yellowtail.Data.Entities;

namespace Yellowtail.Data;

/// <summary>
/// The Entity Framework Core database context for the Yellowtail membership system.
/// </summary>
public class YellowtailDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="YellowtailDbContext"/> class.
    /// </summary>
    /// <param name="options">The options used to configure this context, including the connection string.</param>
    public YellowtailDbContext(DbContextOptions<YellowtailDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// The members table.
    /// </summary>
    public DbSet<Member> Members => Set<Member>();

    /// <summary>
    /// The sports table (global catalog).
    /// </summary>
    public DbSet<Sport> Sports => Set<Sport>();

    /// <summary>
    /// The member-sport association table.
    /// </summary>
    public DbSet<MemberSport> MemberSports => Set<MemberSport>();

    /// <summary>
    /// Configures the entity model: constraints, relationships, query filters, and seed data.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(m => m.LastName).IsRequired().HasMaxLength(100);
            entity.Property(m => m.Email).IsRequired().HasMaxLength(256);
            entity.Property(m => m.Phone).HasMaxLength(30);
            entity.Property(m => m.PhotoUrl).HasMaxLength(2048);
            entity.Property(m => m.Role).HasConversion<string>().HasMaxLength(20);

            // Default-hidden for soft-deleted/inactive members; explicit reads (GetById,
            // update, delete) opt out via IgnoreQueryFilters. Mirrors the tenant-filter
            // convention planned for Phase 2 so the mechanism is already familiar.
            entity.HasQueryFilter(m => m.IsActive);
        });

        modelBuilder.Entity<Sport>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(s => s.Name).IsUnique();

            entity.HasData(
                new Sport { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), Name = "Tennis" },
                new Sport { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), Name = "Football" },
                new Sport { Id = Guid.Parse("00000000-0000-0000-0000-000000000003"), Name = "Swimming" },
                new Sport { Id = Guid.Parse("00000000-0000-0000-0000-000000000004"), Name = "Basketball" },
                new Sport { Id = Guid.Parse("00000000-0000-0000-0000-000000000005"), Name = "Padel" });
        });

        modelBuilder.Entity<MemberSport>(entity =>
        {
            entity.HasKey(ms => new { ms.MemberId, ms.SportId });

            entity.HasOne(ms => ms.Member)
                .WithMany(m => m.MemberSports)
                .HasForeignKey(ms => ms.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ms => ms.Sport)
                .WithMany(s => s.MemberSports)
                .HasForeignKey(ms => ms.SportId)
                .OnDelete(DeleteBehavior.Cascade);

            // Matches Member's filter so a soft-deleted member's rows don't leak
            // through this side of the relationship either.
            entity.HasQueryFilter(ms => ms.Member.IsActive);
        });
    }
}
