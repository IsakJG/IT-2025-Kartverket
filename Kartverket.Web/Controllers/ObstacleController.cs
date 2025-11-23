using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Kartverket.Web.Models;
using System.Text.Json;
using Kartverket.Web.Data;
using Kartverket.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kartverket.Web.Controllers;

public class ObstacleController : Controller
{
    private readonly KartverketDbContext _context;

    public ObstacleController(KartverketDbContext context)
    {
        _context = context;
    }
    // blir kalt etter at vi trykker på "Register Obstacle" lenken i Index viewet
    [HttpGet]
    public ActionResult DataForm() 
    {
        return View(new ObstacleData());
    }


    // blir kalt etter at vi trykker på "Submit Data" knapp i DataForm viewet
   /* [HttpPost]
    public ActionResult DataForm(ObstacleData obstacledata) // POST
    {
        bool isDraft = false;
        if (obstacledata.ObstacleDescription == null)
        {
            isDraft = true;
        }

        return View("Overview", obstacledata);
    }
    
    public IActionResult Map()
    {
        return View();
    }*/
    // POST: mottar skjemaet og lagrer en Report i databasen
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DataForm(ObstacleData obstacledata)
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (!ModelState.IsValid)
        {
            return View(obstacledata);
        }
        //Lag timestamp for rapporten 
        var ts  = new TimestampEntry();
        _context.Timestamps.Add(ts);

        //Lage posisjon fra skjema som Geiography string
        var geoJson = JsonSerializer.Serialize(new
        {
                lat = obstacledata.Latitude ,
              lng =  obstacledata.Longitude
              
        });

        var report = new Report
        {
            Title = obstacledata.ObstacleName,
            Description = obstacledata.ObstacleDescription,
            HeightInFeet = obstacledata.ObstacleHeight, // cast double -> nullable short
            GeoLocation = geoJson,

            //Setter standardverdier for nå - kan ednres senere der det kommer fra den piloten som er logget inn
            StatusId = 1,      // 1 = Pending (fra seed)
            CategoryId = 1,    // 1 = "Obstacle" (fra seed)
            UserId = userId,        // Henter fra session
            TimestampEntry = ts
        };
        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        // Vis en bekreftelsesside eller gå til Archive
        ViewBag.ReportId = report.ReportId;   // kan brukes i Overview hvis du vil
        return View("Overview", obstacledata);
        // Eller, hvis du heller vil rett til arkivet:
        // return RedirectToAction("Archive", "Report");

    }
}
