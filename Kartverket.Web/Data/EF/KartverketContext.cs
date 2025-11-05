using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Kartverket.Web.Data.EF;

public partial class KartverketContext : DbContext
{
    public KartverketContext(DbContextOptions<KartverketContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<DateTimestamp> DateTimestamps { get; set; }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<Organisation> Organisations { get; set; }

    public virtual DbSet<Report> Report { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_unicode_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryID).HasName("PRIMARY");

            entity.Property(e => e.CategoryID).HasColumnType("int(11)");
            entity.Property(e => e.CategoryName).HasMaxLength(50);
        });

        modelBuilder.Entity<DateTimestamp>(entity =>
        {
            entity.HasKey(e => e.DateID).HasName("PRIMARY");

            entity.ToTable("DateTimestamp");

            entity.Property(e => e.DateID).HasColumnType("int(11)");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp");
            entity.Property(e => e.DateOfLastChange).HasColumnType("timestamp");
        });

        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.ImageID).HasName("PRIMARY");

            entity.Property(e => e.ImageID).HasColumnType("int(11)");
            entity.Property(e => e.ImageHeight).HasColumnType("int(11)");
            entity.Property(e => e.ImageLength).HasColumnType("int(11)");
            entity.Property(e => e.ImageURL).HasMaxLength(255);
        });

        modelBuilder.Entity<Organisation>(entity =>
        {
            entity.HasKey(e => e.OrgID).HasName("PRIMARY");

            entity.ToTable("Organisation");

            entity.Property(e => e.OrgID).HasColumnType("int(11)");
            entity.Property(e => e.OrgName).HasMaxLength(50);
        });
        

        modelBuilder.Entity<Report>(entity =>
        {
            entity.Property(e => e.GeoLocation).HasColumnType("point");

            entity.HasKey(e => e.ReportID).HasName("PRIMARY");

            entity.ToTable("Report");

            entity.HasIndex(e => e.AssignedToUser, "AssignedToUser");

            entity.HasIndex(e => e.CategoryID, "CategoryID");

            entity.HasIndex(e => e.DateID, "DateID");

            entity.HasIndex(e => e.DecisionByUser, "DecisionByUser");

            entity.HasIndex(e => e.ImageID, "ImageID");

            entity.HasIndex(e => e.StatusID, "StatusID");

            entity.HasIndex(e => e.UserID, "UserID");

            entity.Property(e => e.ReportID).HasColumnType("int(11)");
            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp");
            entity.Property(e => e.AssignedToUser).HasColumnType("int(11)");
            entity.Property(e => e.CategoryID).HasColumnType("int(11)");
            entity.Property(e => e.DateID).HasColumnType("int(11)");
            entity.Property(e => e.DecisionAt).HasColumnType("timestamp");
            entity.Property(e => e.DecisionByUser).HasColumnType("int(11)");
            entity.Property(e => e.Feedback).HasColumnType("mediumtext");
            entity.Property(e => e.HeightInFeet).HasPrecision(8, 1);
            entity.Property(e => e.ImageID).HasColumnType("int(11)");
            entity.Property(e => e.ReportDescription).HasColumnType("mediumtext");
            entity.Property(e => e.StatusID).HasColumnType("int(11)");
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.UserID).HasColumnType("int(11)");

            entity.HasOne(d => d.AssignedToUserNavigation).WithMany(p => p.ReportAssignedToUserNavigations)
                .HasForeignKey(d => d.AssignedToUser)
                .HasConstraintName("Report_ibfk_6");

            entity.HasOne(d => d.Category).WithMany(p => p.Reports)
                .HasForeignKey(d => d.CategoryID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Report_ibfk_4");

            entity.HasOne(d => d.Date).WithMany(p => p.Reports)
                .HasForeignKey(d => d.DateID)
                .HasConstraintName("Report_ibfk_5");

            entity.HasOne(d => d.DecisionByUserNavigation).WithMany(p => p.ReportDecisionByUserNavigations)
                .HasForeignKey(d => d.DecisionByUser)
                .HasConstraintName("Report_ibfk_7");

            entity.HasOne(d => d.Image).WithMany(p => p.Reports)
                .HasForeignKey(d => d.ImageID)
                .HasConstraintName("Report_ibfk_2");

            entity.HasOne(d => d.Status).WithMany(p => p.Reports)
                .HasForeignKey(d => d.StatusID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Report_ibfk_3");

            entity.HasOne(d => d.User).WithMany(p => p.ReportUsers)
                .HasForeignKey(d => d.UserID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Report_ibfk_1");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleID).HasName("PRIMARY");

            entity.ToTable("Role");

            entity.Property(e => e.RoleID).HasColumnType("int(11)");
            entity.Property(e => e.RoleDescription).HasColumnType("tinytext");
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.StatusID).HasName("PRIMARY");

            entity.Property(e => e.StatusID).HasColumnType("int(11)");
            entity.Property(e => e.StatusName).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserID).HasName("PRIMARY");

            entity.HasIndex(e => e.Email, "Email").IsUnique();

            entity.HasIndex(e => e.OrgID, "fk_user_org");

            entity.Property(e => e.UserID).HasColumnType("int(11)");
            entity.Property(e => e.OrgID).HasColumnType("int(11)");
            entity.Property(e => e.PasswordSalt).HasMaxLength(255);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Org).WithMany(p => p.Users)
                .HasForeignKey(d => d.OrgID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_user_org");

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRole",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleID")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("UserRoles_ibfk_2"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserID")
                        .HasConstraintName("UserRoles_ibfk_1"),
                    j =>
                    {
                        j.HasKey("UserID", "RoleID")
                            .HasName("PRIMARY")
                            .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                        j.ToTable("UserRoles");
                        j.HasIndex(new[] { "RoleID" }, "RoleID");
                        j.IndexerProperty<int>("UserID").HasColumnType("int(11)");
                        j.IndexerProperty<int>("RoleID").HasColumnType("int(11)");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
