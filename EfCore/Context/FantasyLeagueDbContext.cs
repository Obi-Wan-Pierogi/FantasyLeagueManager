using System;
using System.Collections.Generic;
using FantasyLeagueManager.EfCore.Entities;
using Microsoft.EntityFrameworkCore;

namespace FantasyLeagueManager.EfCore.Context;

public partial class FantasyLeagueDbContext : DbContext
{
    public FantasyLeagueDbContext()
    {
    }

    public FantasyLeagueDbContext(DbContextOptions<FantasyLeagueDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Player> Players { get; set; }

    public virtual DbSet<RosterLog> RosterLogs { get; set; }

    public virtual DbSet<Team> Teams { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=DESKTOP-L00PKD4\\MYSERVER;Database=FantasyLeagueDb;Integrated Security=True;TrustServerCertificate=True;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.PlayerId).HasName("PK__Players__4A4E74C83493E8D8");
        });

        modelBuilder.Entity<RosterLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__RosterLo__5E548648F7D6C73C");

            entity.HasOne(d => d.Player)
                .WithMany() 
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RosterLogs_Players");

            entity.Property(e => e.LogDate).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.TeamId).HasName("PK__Teams__123AE799553F5914");
        });

        // --------------------------------------------------------
        // WEEK 8: Fluent API Relationship Configuration
        // --------------------------------------------------------
        modelBuilder.Entity<Player>()
            .HasOne(p => p.TeamNavigation) // A Player has ONE Team
            .WithMany(t => t.Players)      // A Team has MANY Players
            .HasForeignKey(p => p.TeamId)  // The Foreign Key is TeamId
            .HasConstraintName("FK_Players_Teams"); // Explicitly naming the constraint in SQL

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
