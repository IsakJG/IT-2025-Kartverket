using Microsoft.EntityFrameworkCore;
using Kartverket.Web.Models.Entities;

namespace Kartverket.Web.Data
{
    /// <summary>
    /// Databasekontekst for Kartverket-applikasjonen.
    /// Håndterer konfigurasjon av tabeller, relasjoner og skjemaregler.
    /// </summary>
    public class KartverketDbContext : DbContext
    {
        public KartverketDbContext(DbContextOptions<KartverketDbContext> options)
            : base(options)
        {
        }

        // DbSets representerer tabellene i databasen
        public DbSet<Report> Reports { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Organization> Organization { get; set; } // Merk: Burde ideelt sett hete "Organizations" (flertall)
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<TimestampEntry> Timestamps { get; set; }

        /// <summary>
        /// Konfigurerer databasemodellen, primærnøkler, relasjoner og begrensninger (Fluent API).
        /// </summary>
        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            #region Organization & Users

            b.Entity<Organization>(e =>
            {
                e.ToTable("Organization");
                e.HasKey(o => o.OrgId);
                
                e.Property(o => o.OrgName)
                    .HasMaxLength(50)
                    .IsRequired();

                // 1-to-many relationship with User 
                // Restrict delete: Vi kan ikke slette en organisasjon hvis den har ansatte.
                e.HasMany(o => o.Users)
                    .WithOne(u => u.Organization)
                    .HasForeignKey(u => u.OrgId)
                    .OnDelete(DeleteBehavior.Restrict); 
            });

            b.Entity<User>(e =>
            {
                e.ToTable("User");
                e.HasKey(u => u.UserId);

                e.Property(u => u.Username).HasMaxLength(50).IsRequired();
                e.Property(u => u.Email).HasMaxLength(255).IsRequired();
                e.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
            });

            #endregion

            #region Roles & Access Control

            b.Entity<Role>(e =>
            {
                e.ToTable("Role");
                e.HasKey(r => r.RoleId);
                e.Property(r => r.RoleName).HasMaxLength(50).IsRequired();
            });

            b.Entity<UserRole>(e =>
            {
                e.ToTable("UserRole");
                
                // Composite Primary Key (Mange-til-mange koblingstabell)
                e.HasKey(ur => new { ur.UserId, ur.RoleId });

                e.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade); // Sletter vi brukeren, ryker koblingen

                e.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            #endregion

            #region Lookups (Status, Category, Image)

            b.Entity<Status>(e =>
            {
                e.ToTable("Status");
                e.HasKey(s => s.StatusId);
                e.Property(s => s.StatusName).HasMaxLength(50).IsRequired();
            });

            b.Entity<Category>(e =>
            {
                e.ToTable("Category");
                e.HasKey(c => c.CategoryId);
                e.Property(c => c.CategoryName).HasMaxLength(50).IsRequired();
            });

            b.Entity<Image>(e =>
            {
                e.ToTable("Image");
                e.HasKey(i => i.ImageId);
                e.Property(i => i.ImageUrl).HasMaxLength(255).IsRequired();
            });

            #endregion

            #region Timestamp Logic

            b.Entity<TimestampEntry>(e =>
            {
                e.ToTable("TimestampEntry");
                e.HasKey(t => t.DateId);

                // SQL Default Values
                e.Property(t => t.DateCreated)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                e.Property(t => t.DateOfLastChange)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            #endregion

            #region Report Configuration

            b.Entity<Report>(e =>
            {
                e.ToTable("Report");
                e.HasKey(x => x.ReportId);
                
                e.Property(r => r.Title)
                    .HasMaxLength(255);

                // Default status: 1 (Pending)
                e.Property(x => x.StatusId)
                    .HasDefaultValue(1); 

                // Konfigurer relasjoner med SetNull 
                // (Hvis en bruker/status slettes, beholdes rapporten men feltet settes til NULL)
                
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

            #endregion
        }
    }
}