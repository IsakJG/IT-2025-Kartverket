using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Kartverket.Web.Data;
using Kartverket.Web.Models.Entities;
using System.Text.Json;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Setter kultur til en-US for å bruke engelsk tallformat
var cultureInfo = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Remove Kestrel server header for security
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.AddServerHeader = false;
});

builder.AddServiceDefaults();

var connectionString =
    builder.Configuration.GetConnectionString("kartverketdb")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string for database not found.");

var serverVersion = new MySqlServerVersion(new Version(10, 11, 0)); // f.eks. MariaDB 10.11

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

// 🔒 LEGG TIL HSTS KONFIGURASJON
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
    options.ExcludedHosts.Clear();
});

// 🔒 FORBEDRET SESSION KONFIGURASJON MED SIKKERHET
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true; // Forhindrer JavaScript tilgang
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Kun HTTPS
    options.Cookie.SameSite = SameSiteMode.Strict; // CSRF-beskyttelse
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "Kartverket.Session";
});

// 🔒 ANTI-FORGERY KONFIGURASJON
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.SuppressXFrameOptionsHeader = false;
});

var app = builder.Build();

// 🔒 BRUK HSTS I PRODUKSJON
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

// 🔒 SECURITY HEADERS MIDDLEWARE
app.Use(async (context, next) =>
{
    // Content Security Policy
    context.Response.Headers.Append("Content-Security-Policy", 
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' https://unpkg.com https://cdn.tailwindcss.com; " +
        "style-src 'self' 'unsafe-inline' https://unpkg.com https://cdn.tailwindcss.com; " +
        "img-src 'self' data: https:; " +
        "connect-src 'self'; " +
        "font-src 'self'; " +
        "object-src 'none'; " +
        "base-uri 'self'; " +
        "form-action 'self';");
    
    // XSS Protection
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    
    // MIME-type sniffing beskyttelse
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    
    // Clickjacking beskyttelse
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    
    // Referrer Policy
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    
    // Permissions Policy
    context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
    
    await next();
});

app.UseStaticFiles();
app.UseRouting();

// 🔒 BRUK SESSION MIDDLEWARE FØR AUTHORIZATION
app.UseSession();

app.UseAuthorization();

// Database seeding og migrering
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
            lat = 7.955571,
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
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// 🔒 ERROR HANDLING FOR PRODUKSJON
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.Run();