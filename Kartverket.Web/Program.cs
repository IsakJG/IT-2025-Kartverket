using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args); // Oppretter en WebApplicationBuilder

// Setter kultur til en-US for å bruke engelsk tallformat
var cultureInfo = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

builder.AddServiceDefaults(); // Legger til standard tjenester for .NET Aspire

// Add services to the container.
builder.Services.AddControllersWithViews(); // Legger til støtte for kontroller med visninger
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme) // Bruk av CookieAuthenticationDefaults.AuthenticationScheme
    .AddCookie(options =>
    {
        options.Cookie.Name = "Kartverket.CookieAuth"; // Setter egendefinert navn for cookien
        options.LoginPath = "/Account/Login"; // Angir innloggingsstien
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Setter utløpstid for cookien
    });
builder.AddMySqlDataSource(connectionName: "mysqldb"); // Legger til MySQL-datakilde med navnet "mysqldb"
var app = builder.Build(); // Bygger applikasjonen

/*
// Dependency Injection (Register services)
// Get connection string directly from configuration in appsetting.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Register your service that uses the connection
builder.Services.AddSingleton(new MySqlConnection(connectionString));
*/


app.MapDefaultEndpoints(); // Legger til standard endepunkter

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection(); // Legger til HTTPS-omdirigering
app.UseRouting(); // Legger til ruting
app.UseAuthentication(); // Legger til autentisering
app.UseAuthorization(); // Legger til autorisering

app.MapStaticAssets(); // Legger til statiske filer

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets(); // Legger til standard rute for kontroller


app.Run(); // Starter applikasjonen
