using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Kartverket.Web.Data;
using Kartverket.Web.Models;
using Kartverket.Web.Models.Entities;

namespace Kartverket.Web.Controllers;

/// <summary>
/// Kontroller for "Pilot"-rollen. Håndterer registrering, lagring og redigering av hindre (Obstacles).
/// Støtter funksjonalitet for å lagre uferdige utkast (Drafts).
/// </summary>
[Authorize(Roles = "Pilot")]
public class ObstacleController : Controller
{
    private readonly KartverketDbContext _context;
    private readonly ILogger<ObstacleController> _logger;

    // Konstanter for å unngå "Magic Numbers/Strings"
    private const string SessionKeyUserId = "UserId";
    private const string ActionSubmit = "submit";
    private const string ActionDraft = "draft";
    
    // Status ID-er (Bør ideelt sett ligge i en Enum, men konstanter fungerer bra her)
    private const int StatusDraft = 4;
    private const int StatusPending = 1;
    private const int DefaultCategoryId = 1;

    public ObstacleController(KartverketDbContext context, ILogger<ObstacleController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Viser et tomt skjema for ny registrering.
    /// </summary>
    [HttpGet]
    public IActionResult DataForm() 
    {
        return View(new ObstacleData());
    }

    /// <summary>
    /// Viser liste over innlogget brukers utkast (Drafts).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> MyDrafts()
    {
        var userId = GetCurrentUserId();
        
        var drafts = await _context.Reports
            .AsNoTracking() // Bedre ytelse for skrivebeskyttet liste
            .Include(r => r.TimestampEntry)
            .Where(r => r.UserId == userId && r.StatusId == StatusDraft) 
            .OrderByDescending(r => r.TimestampEntry.DateCreated)
            .ToListAsync();

        return View(drafts);
    }

    /// <summary>
    /// Laster inn et spesifikt utkast i skjemaet for redigering.
    /// Parser GeoJSON for å sette startposisjon i kartet.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> EditDraft(int id)
    {
        var userId = GetCurrentUserId();

        var report = await _context.Reports
            .FirstOrDefaultAsync(r => r.ReportId == id && r.UserId == userId);

        if (report == null) 
        {
            _logger.LogWarning("Bruker {UserId} forsøkte å redigere rapport {ReportId} som ikke finnes eller ikke tilhører dem.", userId, id);
            return NotFound();
        }

        var model = new ObstacleData
        {
            ReportId = report.ReportId,
            ObstacleName = report.Title,
            ObstacleDescription = report.Description,
            ObstacleHeight = report.HeightInFeet == 0 ? null : report.HeightInFeet,
            GeometryGeoJson = report.GeoLocation
        };

        // Forsøk å hente senterpunkt (lat/lng) fra lagret data for å sentrere kartet i viewet
        ExtractCoordinatesFromJson(report.GeoLocation, model);

        return View("DataForm", model);
    }

    /// <summary>
    /// Behandler lagring av rapporten. Håndterer både "Lagre som utkast" og "Send inn".
    /// </summary>
    /// <param name="model">Data fra skjemaet.</param>
    /// <param name="action">Knappen brukeren trykket på ("draft" eller "submit").</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveReport(ObstacleData model, string action)
    {
        var userId = GetCurrentUserId();
        
        // --- 1. Valideringslogikk ---
        if (action == ActionSubmit)
        {
            // Streng validering ved innsending
            if (string.IsNullOrWhiteSpace(model.ObstacleName)) 
                ModelState.AddModelError(nameof(model.ObstacleName), "Name is required.");
            if (!model.ObstacleHeight.HasValue) 
                ModelState.AddModelError(nameof(model.ObstacleHeight), "Height is required.");
        }
        else 
        {
            // Slapp validering for utkast (tillater lagring av uferdige data)
            ModelState.Clear();
             if (string.IsNullOrWhiteSpace(model.ObstacleName)) 
                ModelState.AddModelError(nameof(model.ObstacleName), "Name is required.");
        }

        if (!ModelState.IsValid)
        {
            return View("DataForm", model);
        }

        try
        {
            // --- 2. Hent eller Opprett Entitet ---
            Report report;
            TimestampEntry timestamp;

            if (model.ReportId.HasValue)
            {
                // Oppdater eksisterende utkast
                report = await _context.Reports
                    .Include(r => r.TimestampEntry)
                    .FirstOrDefaultAsync(r => r.ReportId == model.ReportId && r.UserId == userId);
            
                if (report == null) return NotFound();
            
                timestamp = report.TimestampEntry;
                timestamp.DateOfLastChange = DateTime.Now;
            }
            else
            {
                // Opprett ny rapport
                timestamp = new TimestampEntry { DateCreated = DateTime.Now, DateOfLastChange = DateTime.Now };
                _context.Timestamps.Add(timestamp);
            
                report = new Report
                {
                    UserId = userId,
                    TimestampEntry = timestamp
                };
                _context.Reports.Add(report);
            }

            // --- 3. Map Data ---
            report.Title = model.ObstacleName;
            report.Description = model.ObstacleDescription;
            report.HeightInFeet = model.ObstacleHeight ?? 0;

            // Håndtering av Geometri
            if (!string.IsNullOrEmpty(model.GeometryGeoJson))
            {
                report.GeoLocation = model.GeometryGeoJson;
            }
            else
            {
                // Fallback: Lagre enkel markørposisjon hvis ingen tegning finnes
                report.GeoLocation = JsonSerializer.Serialize(new { lat = model.Latitude, lng = model.Longitude });
            }
            // Oppdater modellens Latitude/Longitude fra faktisk GeoLocation,
            // slik at Overview-siden får riktig startpunkt (ikke 0,0).
            ExtractCoordinatesFromJson(report.GeoLocation, model);
            // --- 4. Sett Status og Lagre ---
            if (action == ActionDraft)
            {
                report.StatusId = StatusDraft;
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Bruker {UserId} lagret utkast {ReportId}.", userId, report.ReportId);
                TempData["SuccessMessage"] = "Report saved as draft.";
                return RedirectToAction(nameof(MyDrafts));
            }
            else // Action == Submit
            {
                report.StatusId = StatusPending;
                report.CategoryId = DefaultCategoryId;
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Bruker {UserId} sendte inn rapport {ReportId} til godkjenning.", userId, report.ReportId);
                
                ViewBag.ReportId = report.ReportId;
                return View("Overview", model);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Feil ved lagring av rapport for bruker {UserId}", userId);
            ModelState.AddModelError(string.Empty, "An error occurred while saving. Please try again.");
            return View("DataForm", model);
        }
    }

    /// <summary>
    /// Sletter et utkast permanent.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDraft(int id)
    {
        var userId = GetCurrentUserId();

        var report = await _context.Reports
            .FirstOrDefaultAsync(r => r.ReportId == id && r.UserId == userId && r.StatusId == StatusDraft); 

        if (report == null)
        {
            TempData["ErrorMessage"] = "Draft not found or permission denied.";
            return RedirectToAction(nameof(MyDrafts));
        }

        try
        {
            // Slett timestamp manuelt hvis Cascade Delete ikke er konfigurert i databasen
            if (report.DateId.HasValue)
            {
                var timestamp = await _context.Timestamps.FindAsync(report.DateId.Value);
                if (timestamp != null) _context.Timestamps.Remove(timestamp);
            }

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Bruker {UserId} slettet utkast {ReportId}.", userId, id);
            TempData["SuccessMessage"] = $"Draft #{id} was deleted.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Feil ved sletting av utkast {ReportId}", id);
            TempData["ErrorMessage"] = "Could not delete draft due to a server error.";
        }

        return RedirectToAction(nameof(MyDrafts));
    }

    // --- Private Helper Methods ---

    /// <summary>
    /// Henter innlogget bruker-ID fra session. Returnerer 0 hvis ikke funnet (skal fanges opp av Authorize).
    /// </summary>
    private int GetCurrentUserId()
    {
        return HttpContext.Session.GetInt32(SessionKeyUserId) ?? 0;
    }

    /// <summary>
    /// Forsøker å hente ut lat/lng fra en JSON-streng for å populere modellen.
    /// </summary>
    private void ExtractCoordinatesFromJson(string? json, ObstacleData model)
{
    if (string.IsNullOrWhiteSpace(json)) return;

    try
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (root.ValueKind != JsonValueKind.Object)
            return;

        // CASE 1: Enkel JSON: { "lat": X, "lng": Y }
        if (root.TryGetProperty("lat", out var latProp) &&
            root.TryGetProperty("lng", out var lngProp) &&
            latProp.ValueKind == JsonValueKind.Number &&
            lngProp.ValueKind == JsonValueKind.Number)
        {
            model.Latitude = latProp.GetDouble();
            model.Longitude = lngProp.GetDouble();
            return;
        }

        // CASE 2: FeatureCollection -> gå ned til geometry
        if (root.TryGetProperty("type", out var typeProp) &&
            typeProp.ValueKind == JsonValueKind.String &&
            typeProp.GetString() == "FeatureCollection" &&
            root.TryGetProperty("features", out var featuresProp) &&
            featuresProp.ValueKind == JsonValueKind.Array)
        {
            var firstFeature = featuresProp.EnumerateArray().FirstOrDefault();
            if (firstFeature.ValueKind == JsonValueKind.Object &&
                firstFeature.TryGetProperty("geometry", out var geometryProp) &&
                geometryProp.ValueKind == JsonValueKind.Object)
            {
                root = geometryProp;
            }
        }

        // CASE 3: GeoJSON Point eller LineString
        if (!root.TryGetProperty("type", out var geomTypeProp) ||
            geomTypeProp.ValueKind != JsonValueKind.String ||
            !root.TryGetProperty("coordinates", out var coordsProp) ||
            coordsProp.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        var geomType = geomTypeProp.GetString();

        if (geomType == "Point")
        {
            // coordinates: [lon, lat]
            var coords = coordsProp.EnumerateArray().ToList();
            if (coords.Count >= 2 &&
                coords[0].ValueKind == JsonValueKind.Number &&
                coords[1].ValueKind == JsonValueKind.Number)
            {
                var lng = coords[0].GetDouble();
                var lat = coords[1].GetDouble();

                model.Latitude = lat;
                model.Longitude = lng;
            }
        }
        else if (geomType == "LineString")
        {
            // coordinates: [[lon, lat], [lon, lat], ...]
            var all = coordsProp.EnumerateArray().ToList();
            if (all.Count >= 1)
            {
                var first = all[0].EnumerateArray().ToList();
                if (first.Count >= 2 &&
                    first[0].ValueKind == JsonValueKind.Number &&
                    first[1].ValueKind == JsonValueKind.Number)
                {
                    var lng = first[0].GetDouble();
                    var lat = first[1].GetDouble();

                    model.Latitude = lat;
                    model.Longitude = lng;
                }
            }
        }
    }
    catch (JsonException ex)
    {
        _logger.LogWarning(ex, "Kunne ikke parse GeoJSON for rapport {ReportId}", model.ReportId);
    }
}
}