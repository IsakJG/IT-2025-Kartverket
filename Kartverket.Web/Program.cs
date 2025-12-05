using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Kartverket.Web.Data;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURATION ---

// Sett kultur til en-US for korrekt håndtering av punktum i koordinater (lat/lng)
var cultureInfo = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Sikkerhet: Fjern "Server: Kestrel" header for å ikke avsløre serverteknologi
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.AddServerHeader = false;
});

// Database Connection
var connectionString = builder.Configuration.GetConnectionString("kartverketdb")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Database connection string not found.");

// Bruker MariaDB/MySQL (Juster versjon etter din server)
var serverVersion = new MySqlServerVersion(new Version(10, 11, 0));

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

builder.Services.AddControllersWithViews();

// Sikkerhet: HSTS (Strict Transport Security)
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
    options.ExcludedHosts.Clear();
});

// Sikkerhet: Session Hardening
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60); // Økt til 1 time
    options.Cookie.HttpOnly = true;     // Hindrer XSS i å stjele session ID
    options.Cookie.IsEssential = true;  // GDPR: Nødvendig cookie
    options.Cookie.Name = ".Kartverket.Session";
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Krever HTTPS
});

// Sikkerhet: CSRF/XSRF beskyttelse
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN"; // For AJAX-kall
});

var app = builder.Build();

// --- 2. PIPELINE (MIDDLEWARE) ---

// Feilhåndtering bør ligge ØVERST i pipelinen for å fange alle feil
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // Tvinger nettlesere til å bruke HTTPS
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

// Sikkerhets-headere (CSP, XSS, etc.)
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    
    // Tillater Leaflet (unpkg) og Tailwind (cdn)
    context.Response.Headers.Append("Content-Security-Policy", 
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://unpkg.com https://cdn.tailwindcss.com; " +
        "style-src 'self' 'unsafe-inline' https://unpkg.com https://cdn.tailwindcss.com; " +
        "img-src 'self' data: https: *.openstreetmap.org; " + // Tillat OpenStreetMap
        "connect-src 'self'; " +
        "font-src 'self'; " +
        "base-uri 'self'; " +
        "form-action 'self';");

    context.Response.Headers.Append("Permissions-Policy", "geolocation=(self)"); // Tillat kart-lokasjon
    
    await next();
});

app.UseStaticFiles();
app.UseRouting();

// VIKTIG: Session må komme før Authentication
app.UseSession();

// Egendefinert Middleware: Oversetter Session-data til ClaimsPrincipal (Brukeridentitet)
app.Use(async (context, next) =>
{
    var userId = context.Session.GetInt32("UserId");
    var rolesString = context.Session.GetString("UserRoles");
    var username = context.Session.GetString("Username");

    if (userId.HasValue && !string.IsNullOrEmpty(rolesString))
    {
        var claims = new List<System.Security.Claims.Claim>
        {
            new(System.Security.Claims.ClaimTypes.Name, username ?? ""),
            new(System.Security.Claims.ClaimTypes.NameIdentifier, userId.Value.ToString())
        };

        foreach (var role in rolesString.Split(','))
        {
            claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role.Trim()));
        }

        var identity = new System.Security.Claims.ClaimsIdentity(claims, "SessionAuth");
        context.User = new System.Security.Claims.ClaimsPrincipal(identity);
    }

    await next();
});

app.UseAuthorization();

// --- 3. DATABASE SEEDING ---
// Kjøres i et eget scope ved oppstart
using (var scope = app.Services.CreateScope())
{
    try 
    {
        var db = scope.ServiceProvider.GetRequiredService<KartverketDbContext>();
        // Kaller den nye ryddige klassen vår
        DbInitializer.Seed(db);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "En feil oppstod under seeding av databasen.");
    }
}

// --- 4. ENDPOINTS ---

// Standard rute
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();