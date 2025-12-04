using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Kartverket.Web.Models;
using System.Text.Json;
using Kartverket.Web.Data;
using Kartverket.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Kartverket.Web.Controllers;

[Authorize(Roles = "Pilot")]
public class ObstacleController : Controller
{
    private readonly KartverketDbContext _context;

    public ObstacleController(KartverketDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public ActionResult DataForm() 
    {
        // Viser et tomt skjema for ny registrering
        return View(new ObstacleData());
    }

    // GET: List user drafts
    [HttpGet]
    public async Task<IActionResult> MyDrafts()
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        
        // Henter rapporter med status 4 (Draft) for innlogget bruker
        var drafts = await _context.Reports
            .Include(r => r.TimestampEntry)
            .Where(r => r.UserId == userId && r.StatusId == 4) 
            .OrderByDescending(r => r.TimestampEntry.DateCreated)
            .ToListAsync();

        return View(drafts);
    }

    // GET: Load draft into form
    [HttpGet]
    public async Task<IActionResult> EditDraft(int id)
    {
        int? userId = HttpContext.Session.GetInt32("UserId");

        // Henter utkastet basert på ID og bruker
        var report = await _context.Reports
            .FirstOrDefaultAsync(r => r.ReportId == id && r.UserId == userId);

        if (report == null) return NotFound();

        // Mapper database-modellen til ViewModellen (ObstacleData)
        var model = new ObstacleData
        {
            ReportId = report.ReportId,
            ObstacleName = report.Title,
            ObstacleDescription = report.Description,
            ObstacleHeight = report.HeightInFeet == 0 ? null : report.HeightInFeet,
            GeometryGeoJson = report.GeoLocation
        };

        // Prøver å hente lat/lng for å sentrere kartet riktig
        if (!string.IsNullOrEmpty(report.GeoLocation))
        {
            try
            {
                using var doc = JsonDocument.Parse(report.GeoLocation);
                var root = doc.RootElement;
                
                // Hvis GeoJSON er et enkelt objekt (ikke FeatureCollection)
                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("lat", out var latProp)) 
                        model.Latitude = latProp.GetDouble();
                    
                    if (root.TryGetProperty("lng", out var lngProp)) 
                        model.Longitude = lngProp.GetDouble();
                }
            }
            catch { }
        }

        // Gjenbruker DataForm-viewet, men med data fylt inn
        return View("DataForm", model);
    }

    // POST: Save (Draft or Submit)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveReport(ObstacleData model, string action)
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        
        // Betinget validering: Drafts trenger ikke være komplette
        if (action == "submit")
        {
            if (string.IsNullOrWhiteSpace(model.ObstacleName)) 
                ModelState.AddModelError("ObstacleName", "Name is required.");
            if (!model.ObstacleHeight.HasValue) 
                ModelState.AddModelError("ObstacleHeight", "Height is required.");
        }
        else 
        {
            // Fjerner valideringsfeil for drafts
            ModelState.Clear();
        }

        if (!ModelState.IsValid)
        {
            return View("DataForm", model);
        }

        TimestampEntry ts;
        Report report;

        // Sjekker om vi oppdaterer eksisterende rapport/utkast
        if (model.ReportId.HasValue)
        {
            report = await _context.Reports
                .Include(r => r.TimestampEntry)
                .FirstOrDefaultAsync(r => r.ReportId == model.ReportId && r.UserId == userId);
            
            if (report == null) return NotFound();
            
            ts = report.TimestampEntry;
            ts.DateOfLastChange = DateTime.Now;
        }
        else
        {
            // Oppretter ny rapport
            ts = new TimestampEntry();
            _context.Timestamps.Add(ts);
            
            report = new Report();
            report.UserId = userId;
            report.TimestampEntry = ts;
            _context.Reports.Add(report);
        }

        // Oppdaterer feltene
        report.Title = model.ObstacleName;
        report.Description = model.ObstacleDescription;
        report.HeightInFeet = model.ObstacleHeight ?? 0;
        
        // GeoJSON Logikk
        if (!string.IsNullOrEmpty(model.GeometryGeoJson))
        {
            report.GeoLocation = model.GeometryGeoJson;
        }
        else
        {
            // Fallback hvis ingen tegning, lagrer kun markør
            report.GeoLocation = JsonSerializer.Serialize(new { lat = model.Latitude, lng = model.Longitude });
        }

        // Sjekker hvilken knapp brukeren trykket på
        if (action == "draft")
        {
            report.StatusId = 4; // Draft
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Report saved as draft.";
            return RedirectToAction("MyDrafts");
        }
        else
        {
            report.StatusId = 1; // Pending (Sendt inn)
            report.CategoryId = 1; 
            await _context.SaveChangesAsync();
            
            ViewBag.ReportId = report.ReportId;
            return View("Overview", model);
        }
    }

    // POST: Delete Draft
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDraft(int id)
    {
        int? userId = HttpContext.Session.GetInt32("UserId");

        // Henter rapporten kun hvis den tilhører brukeren OG er et utkast
        var report = await _context.Reports
            .FirstOrDefaultAsync(r => r.ReportId == id && r.UserId == userId && r.StatusId == 4); 

        if (report == null)
        {
            TempData["ErrorMessage"] = "Draft not found or you do not have permission to delete it.";
            return RedirectToAction("MyDrafts");
        }

        try
        {
            // Sletter tilhørende tidsstempel hvis det finnes
            if (report.DateId.HasValue)
            {
                var timestamp = await _context.Timestamps.FindAsync(report.DateId.Value);
                if (timestamp != null)
                {
                    _context.Timestamps.Remove(timestamp);
                }
            }

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Draft #{id} was successfully deleted.";
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "An error occurred while deleting the draft.";
        }

        return RedirectToAction("MyDrafts");
    }
}