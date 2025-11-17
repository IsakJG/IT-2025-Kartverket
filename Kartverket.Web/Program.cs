using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Kartverket.Web.Data;
using Kartverket.Web.Models.Entities;
using System.Text.Json;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Setter kultur til en-US for � bruke engelsk tallformat
var cultureInfo = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

builder.AddServiceDefaults();

var connectionString =
    builder.Configuration.GetConnectionString("kartverketdb")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string for database not found.");

var serverVersion = new MySqlServerVersion(new Version(10, 11, 0)); // f.eks. MariaDB 10.11

//builder.Services.AddDbContext<KartverketDbContext>(options =>
  //  options.UseMySql(connectionString, serverVersion));

  builder.Services.AddDbContext<KartverketDbContext>(options =>
{
    options.UseMySql(connectionString, serverVersion, mySqlOptions =>
    {
        mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
    });
});
// Add services to the container.
builder.Services.AddControllersWithViews();

// NY: Legg til Session support for identitetshåndtering
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "Kartverket.Session";
});

var app = builder.Build();

// NY: Bruk Session middleware før Authorization
app.UseSession();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<KartverketDbContext>();
    db.Database.Migrate();   // Lager tabeller i kartverketdb (eller databasen connection string peker på)

    //Sjekk om Status-tabellen er tom, og legg til standardverdier hvis den er det
    if (!db.Statuses.Any())
    {
        db.Statuses.AddRange(
            new Status { StatusId = 1, StatusName = "Pending" },
            new Status { StatusId = 2, StatusName = "Rejected" },
            new Status { StatusId = 3, StatusName = "Approved" }
        );
    }
    //Category
    if (!db.Categories.Any())
    {
        db.Categories.AddRange(
            new Category { CategoryId = 1, CategoryName = "Obstacle" }
        );
    }
    //Organization
    if (!db.Organization.Any())
    {
        db.Organization.AddRange(
            new Organization { OrgId = 1, OrgName = "Kartverket" },
            new Organization { OrgId = 2, OrgName = "Admin" }
        );
        //Roles
        if (!db.Roles.Any())
        {
            db.Roles.AddRange(
                new Role { RoleId = 1, RoleName = "Admin" },
                new Role { RoleId = 2, RoleName = "Registar" },
                new Role { RoleId = 3, RoleName = "Pilot" }
            );
        }
    }
    //User - OBS: Oppdater til å bruke PasswordHash i stedet for Password!
    if (!db.Users.Any())
    {
        db.Users.AddRange(
            new User
            {
                UserId = 1,
                OrgId = 2,
                Username = "Admin",
                Email = "admin@example.com",
                PasswordHash = Kartverket.Web.Services.PasswordHasher.HashPassword("Admin123") // Endret til PasswordHash
            },
            new User
            {
                UserId = 2,
                OrgId = 1,
                Username = "Registar",
                Email = "Registar@example.com",
                PasswordHash = Kartverket.Web.Services.PasswordHasher.HashPassword("Registar123") // Endret til PasswordHash
            },
            new User
            {
                UserId = 3,
                OrgId = 1,
                Username = "Pilot",
                Email = "Pilot@example.com",
                PasswordHash = Kartverket.Web.Services.PasswordHasher.HashPassword("Pilot123") // Endret til PasswordHash
            }
        );
    }
    //UserRole
    if (!db.UserRoles.Any())
    {
        db.UserRoles.AddRange(
            new UserRole { UserId = 1, RoleId = 1 }, // admin → Admin
            new UserRole { UserId = 2, RoleId = 2 }, // registar → Registar
            new UserRole { UserId = 3, RoleId = 3 }  // pilot → Pilot
        );
    }
    //timestampentry
    if (!db.Timestamps.Any())
    {
        var ts = new TimestampEntry();   // ikke sett DateId eller datoer
        db.Timestamps.Add(ts);
        db.SaveChanges();  
    }

    var firstTimestamp = db.Timestamps.First().DateId;
    //report
    if (!db.Reports.Any())
    {
        var geoJson = JsonSerializer.Serialize(new
        {
            lat =  7.955571,
            lng = 58.112380
        });

        db.Reports.Add(new Report
        {
            Title = "Test obstacle report",
            Description = "Seed test with GeoLocation",
            HeightInFeet = 160,
            GeoLocation = geoJson,

            // FK-er
            StatusId = 1,       // New
            CategoryId = 1,     // Obstacle
            UserId = 1,         // created by admin
            ImageId = null,
            DateId = firstTimestamp
        });
    }
    db.SaveChanges();
}

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Legg til denne for statiske filer (CSS, JS, bilder)
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();