using Microsoft.EntityFrameworkCore;
using Kartverket.Web.Models.Entities;
using Kartverket.Web.Services; // For PasswordHasher
using System.Text.Json;

namespace Kartverket.Web.Data
{
    /// <summary>
    /// Ansvarlig for å seed'e (fylle) databasen med startdata.
    /// Kjøres ved oppstart for å sikre at admin-brukere og nødvendige tabeller eksisterer.
    /// </summary>
    public static class DbInitializer
    {
        public static void Seed(KartverketDbContext db)
        {
            // 1. Sørg for at databasen er opprettet og migrert
            db.Database.Migrate();

            // 2. Seed Lookups (Status, Category, Role)
            if (!db.Statuses.Any())
            {
                db.Statuses.AddRange(
                    new Status { StatusId = 1, StatusName = "Pending" },
                    new Status { StatusId = 2, StatusName = "Rejected" },
                    new Status { StatusId = 3, StatusName = "Approved" },
                    new Status { StatusId = 4, StatusName = "Draft" }
                );
            }

            if (!db.Categories.Any())
            {
                db.Categories.Add(new Category { CategoryId = 1, CategoryName = "Obstacle" });
            }

            if (!db.Roles.Any())
            {
                db.Roles.AddRange(
                    new Role { RoleId = 1, RoleName = "Admin" },
                    new Role { RoleId = 2, RoleName = "Registar" }, // Legacy skrivemåte beholdes
                    new Role { RoleId = 3, RoleName = "Pilot" }
                );
            }

            // 3. Seed Organizations
            if (!db.Organization.Any())
            {
                db.Organization.AddRange(
                    new Organization { OrgId = 1, OrgName = "Kartverket" },
                    new Organization { OrgId = 2, OrgName = "Admin Org" }
                );
            }

            // Lagre metadata først
            db.SaveChanges();

            // 4. Seed Users (med Hashet passord)
            if (!db.Users.Any())
            {
                var users = new List<User>
                {
                    new User
                    {
                        UserId = 1, OrgId = 2, Username = "Admin", Email = "admin@kartverket.no",
                        PasswordHash = PasswordHasher.HashPassword("Admin123"), CreatedAt = DateTime.Now
                    },
                    new User
                    {
                        UserId = 2, OrgId = 1, Username = "Registar", Email = "registar@kartverket.no",
                        PasswordHash = PasswordHasher.HashPassword("Registar123"), CreatedAt = DateTime.Now
                    },
                    new User
                    {
                        UserId = 3, OrgId = 1, Username = "Pilot", Email = "pilot@kartverket.no",
                        PasswordHash = PasswordHasher.HashPassword("Pilot123"), CreatedAt = DateTime.Now
                    }
                };
                db.Users.AddRange(users);
                db.SaveChanges(); // Lagre brukere for å få ID-er hvis autoincrement
            }

            // 5. Seed UserRoles (Koblinger)
            if (!db.UserRoles.Any())
            {
                db.UserRoles.AddRange(
                    new UserRole { UserId = 1, RoleId = 1 }, // Admin -> Admin
                    new UserRole { UserId = 2, RoleId = 2 }, // Registar -> Registar
                    new UserRole { UserId = 3, RoleId = 3 }  // Pilot -> Pilot
                );
                db.SaveChanges();
            }

            // 6. Seed Demo Report
            if (!db.Reports.Any())
            {
                // Opprett timestamp først
                var ts = new TimestampEntry { DateCreated = DateTime.Now, DateOfLastChange = DateTime.Now };
                db.Timestamps.Add(ts);
                db.SaveChanges(); // Få DateId

                var geoJson = JsonSerializer.Serialize(new { lat = 58.164048, lng = 8.004177 }); // Kristiansand

                db.Reports.Add(new Report
                {
                    Title = "Høy byggekran",
                    Description = "Midlertidig kran ved havna. Høyde ca 50 meter.",
                    HeightInFeet = 164,
                    GeoLocation = geoJson,
                    StatusId = 1,       // Pending
                    CategoryId = 1,     // Obstacle
                    UserId = 3,         // Pilot
                    DateId = ts.DateId,
                    AssignedAt = null
                });
                db.SaveChanges();
            }
        }
    }
}