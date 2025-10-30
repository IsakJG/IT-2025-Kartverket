using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Kartverket.Web.Models;
using MySqlConnector;

namespace Kartverket.Web.Controllers;

public class HomeController : Controller 
{
    private readonly ILogger<HomeController> _logger; // Logger for logging information
    private readonly IConfiguration config; // Configuration for accessing app settings

    //private readonly string _connectionString;

    public HomeController(ILogger<HomeController> logger, IConfiguration config) // Konstruktør for HomeController
    {
        _logger = logger; // Initialiserer logger
        this.config = config; // Initialiserer konfigurasjon
    }

    /*
    // Send avhengighet til konstruktøren
        public HomeController(ILogger<HomeController> logger, IConfiguration config)
        {
            _logger = logger;
            _connectionString = config.GetConnectionString("DefaultConnection")!;
        }
    */
    public IActionResult Index() // Hovedsiden
    {
        return View(); // Returnerer Index viewet
    }
/*
//databasen koblingstest 
public async Task<IActionResult> Index()
    {
        try
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            return Content("✅ Connection to MariaDB successful!");
        }
        catch (Exception ex)
        {
            return Content($"❌ Connection failed: {ex.Message}");
        }
        
    }
    */
    public IActionResult GetAThing(int id) // Eksempelmethode som tar en id som parameter
    {
        _logger.LogInformation("GetAThing called with id {Id}", id); // Logger informasjon om kall
        if (id > 10)
        {
            return View(new ThingModel { Name = "Espen" }); // Returnerer ThingModel med navn "Espen" hvis id er større enn 10
        }
        return View(new ThingModel { Name = "Rania" }); // Returnerer ThingModel med navn "Rania" ellers

    }

    public IActionResult MainPage() // En annen side
    {
    return View(); // Returnerer MainPage viewet
    }


    public IActionResult Privacy() // Personvern side
    {
        return View(); // Returnerer Privacy viewet
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)] // Deaktiverer caching for feilsiden
    public IActionResult Error() // Feilsiden
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }); // Returnerer Error viewet med RequestId
    }
}
