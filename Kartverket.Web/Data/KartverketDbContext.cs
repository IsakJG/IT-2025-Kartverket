using Microsoft.EntityFrameworkCore;
using Kartverket.Web.Models.Entities; // her ligger Report, User, osv.

namespace Kartverket.Web.Data;

public class KartverketDbContext : DbContext
{
    public KartverketDbContext(DbContextOptions<KartverketDbContext> options)
        : base(options)
    {
    }

    // Vi starter enkelt: bare Report-tabellen
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Organization> Organization => Set<Organization>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Status> Statuses => Set<Status>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Image> Images => Set<Image>();
    public DbSet<TimestampEntry> Timestamps => Set<TimestampEntry>();


    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // Organization 
        b.Entity<Organization>(e =>
        {
            e.ToTable("Organization");
            e.HasKey(o => o.OrgId);

            e.Property(o => o.OrgName)
            .HasMaxLength(50)
            .IsRequired();

            //1-to-many relationship with User 
            e.HasMany(o => o.Users)
             .WithOne(u => u.Organization)
             .HasForeignKey(u => u.OrgId)
             .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete
        });

        //Role
        b.Entity<Role>(e =>
        {
            e.ToTable("Role");
            e.HasKey(r => r.RoleId);

            e.Property(r => r.RoleName)
            .HasMaxLength(50)
            .IsRequired();
        });

        // User 
        b.Entity<User>(e =>
        {
            e.ToTable("User");
            e.HasKey(u => u.UserId);

            e.Property(u => u.Username)
            .HasMaxLength(50)
            .IsRequired();

            e.Property(u => u.Email)
            .HasMaxLength(255)
            .IsRequired();

            e.Property(u => u.PasswordHash)
            .HasMaxLength(255)
            .IsRequired();
        });

        // UserRole 
        b.Entity<UserRole>(e =>
        {
            e.ToTable("UserRole");
            //Compisite primary key
            e.HasKey(ur => new { ur.UserId, ur.RoleId });

            e.HasOne(ur => ur.User)
             .WithMany(u => u.UserRoles)
             .HasForeignKey(ur => ur.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(ur => ur.Role)
             .WithMany(r => r.UserRoles)
             .HasForeignKey(ur => ur.RoleId)
             .OnDelete(DeleteBehavior.Cascade);

        });

        //status
        b.Entity<Status>(e =>
        {
            e.ToTable("Status");
            e.HasKey(s => s.StatusId);

            e.Property(s => s.StatusName)
            .HasMaxLength(50)
            .IsRequired();
        });

        //category
        b.Entity<Category>(e =>
        {
            e.ToTable("Category");
            e.HasKey(c => c.CategoryId);

            e.Property(c => c.CategoryName)
            .HasMaxLength(50)
            .IsRequired();
        });

        //image
        b.Entity<Image>(e =>
        {
            e.ToTable("Image");
            e.HasKey(i => i.ImageId);

            e.Property(i => i.ImageUrl)
            .HasMaxLength(255)
            .IsRequired();
        });

        //timestampentry
        b.Entity<TimestampEntry>(e =>
        {
            e.ToTable("TimestampEntry");
            e.HasKey(t => t.DateId);

            e.Property(t => t.DateCreated)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

            e.Property(t => t.DateOfLastChange)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Report
        b.Entity<Report>(e =>
        {
            e.ToTable("Report");
            e.HasKey(x => x.ReportId);
            e.Property(r => r.Title)
             .HasMaxLength(255);

            e.Property(x => x.StatusId)
            .HasDefaultValue(1);

            e.HasOne(r => r.User)
             .WithMany(u => u.Reports)
             .HasForeignKey(r => r.UserId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(r => r.AssignedToUser)
                .WithMany(u => u.AssignedReports)
                .HasForeignKey(r => r.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(r => r.DecisionByUser)
                .WithMany(u => u.DecidedReports)
                .HasForeignKey(r => r.DecisionByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(r => r.Image)
            .WithMany(i => i.Reports)
            .HasForeignKey(r => r.ImageId)
            .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(r => r.Status)
            .WithMany(s => s.Reports)
            .HasForeignKey(r => r.StatusId)
            .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(r => r.Category)
            .WithMany(c => c.Reports)
            .HasForeignKey(r => r.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(r => r.TimestampEntry)
            .WithMany(t => t.Reports)
            .HasForeignKey(r => r.DateId)
            .OnDelete(DeleteBehavior.SetNull);

        });

    }
}