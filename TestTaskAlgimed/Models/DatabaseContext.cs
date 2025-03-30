using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TestTaskAlgimed.Models;

public partial class DatabaseContext : DbContext
{
    public DatabaseContext()
    {
        //Database.EnsureDeleted();
        Database.EnsureCreated();
    }

    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Mode> Modes { get; set; }

    public virtual DbSet<Step> Steps { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite("Data Source=C:\\C#\\TestTaskAlgimed\\TestTaskAlgimed\\bin\\Debug\\net8.0-windows\\database.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Mode>(entity =>
        {
            entity.Property(e => e.ID).HasColumnName("ID");
        });

        modelBuilder.Entity<Step>(entity =>
        {
            entity.Property(e => e.ID).HasColumnName("ID");

            entity.HasOne(d => d.Model).WithMany(p => p.Steps)
                .HasForeignKey(d => d.ModeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Login, "IX_Users_Login").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
