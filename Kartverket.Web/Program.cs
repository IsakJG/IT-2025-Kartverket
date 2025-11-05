using System.Globalization;
using Kartverket.Web.Data.EF;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;



var builder = WebApplication.CreateBuilder(args);



// Setter kultur til en-US for � bruke engelsk tallformat
var cultureInfo = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddControllersWithViews();

// EF Core DbContext (bruker "DefaultConnection" fra appsettings.json)
var cs = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<KartverketContext>(opt =>
    opt.UseMySql(cs, ServerVersion.AutoDetect(cs),
        my => my.UseNetTopologySuite())); // Viktig for POINT (GeoLocation)



var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
