using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kartverket.Web.Data;
using Kartverket.Web.Models.Entities;
using Kartverket.Web.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;


[Authorize]
public class ReportController : Controller 
{
    private readonly KartverketDbContext _context;

    public ReportController(KartverketDbContext context)
    {
        _context = context;
    }
    // NY: Active Reports fra database (denne beholdes som han lagde den)
   /* public async Task<IActionResult> ActiveReports() 
    {
        var activeReports = await _context.Reports
            .Include(r => r.User)
            .Include(r => r.Status)
            .Include(r => r.Category)
            .Where(r => r.StatusId == 1) // Pending reports
            .ToListAsync();

        return View(activeReports);
    }
    */

    [Authorize(Roles = "Registar")]
    public async Task<IActionResult> ActiveReports()
{
    var reports = await _context.Reports
        .Include(r => r.User)
        .Include(r => r.Status)
    
        .Include(r => r.TimestampEntry)
        .Where(r => r.StatusId == 1)   // 1 = Pending
        .OrderBy(r => r.ReportId)
        .ToListAsync();

    var rows = reports.Select(r =>
    {
        double? lat = null;
        double? lng = null;

        // Prøv å lese GeoLocation som JSON: { "lat": ..., "lng": ... }
        if (!string.IsNullOrWhiteSpace(r.GeoLocation))
        {
            try
            {
                using var doc = JsonDocument.Parse(r.GeoLocation);
                var root = doc.RootElement;

                if (root.TryGetProperty("lat", out var latProp))
                    lat = latProp.GetDouble();

                if (root.TryGetProperty("lng", out var lngProp))
                    lng = lngProp.GetDouble();
            }
            catch
            {
                // ignorer feil JSON
            }
        }

        string posText;
        if (lat.HasValue && lng.HasValue)
            posText = $"{lat.Value:F5}, {lng.Value:F5}";
        else
            posText = r.GeoLocation ?? "-";

        return new ActiveReportRow
        {
            ReportId  = r.ReportId,
            CreatedBy = r.User?.Username ?? "",
            Status    = r.Status?.StatusName ?? "",
            CreatedAt = r.TimestampEntry?.DateCreated,
            Height    = $"{r.HeightInFeet} ft",
            Position  = posText
        };
    }).ToList();

    return View(rows);   // 👈 nå er modellen List<ActiveReportRow>
}

    //Archive view

    [Authorize(Roles = "Registar, Admin")] //Kun Registrar og Admin kan se hele arkivet
    public async Task<IActionResult> Archive()
    {
        var reports = await _context.Reports
            .Include(r => r.User)
            .Include(r => r.Status)
            .Include(r => r.Category)
            .Include(r => r.TimestampEntry)
            .OrderByDescending(r => r.ReportId)
            .ToListAsync();

        var data = reports.Select(r =>
        {
            double? lat = null;
            double? lng = null;

            // GeoLocation parsing (fra din kode)
            if (!string.IsNullOrWhiteSpace(r.GeoLocation))
            {
                try
                {
                    using var doc = JsonDocument.Parse(r.GeoLocation);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("lat", out var latProp))
                        lat = latProp.GetDouble();

                    if (root.TryGetProperty("lng", out var lngProp))
                        lng = lngProp.GetDouble();
                }
                catch { } // Ignorer feilformatert JSON
            }

            return new ArchiveRow
            {
                ReportID     = r.ReportId,
                Title        = r.Title,
                Pilot        = r.User?.Username,
                Status       = r.Status?.StatusName,
                Category     = r.Category?.CategoryName,
                HeightInFeet = r.HeightInFeet,
                Description  = r.Description,
                Latitude     = lat,
                Longitude    = lng,
                AssignedAt   = r.AssignedAt,
                DecisionAt   = r.DecisionAt,
                CreatedAt    = r.TimestampEntry != null
                               ? r.TimestampEntry.DateCreated
                               : (DateTime?)null
            };
        }).ToList();

        return View(data);
    }
    // NY: Detaljside for en rapport


    public async Task<IActionResult> Details(int id)
    {
        var r = await _context.Reports
            .Include(rep => rep.User)
            .Include(rep => rep.Status)
            .Include(rep => rep.Category)
            .Include(rep => rep.TimestampEntry)
            .FirstOrDefaultAsync(rep => rep.ReportId == id);

        if (r == null)
            return NotFound();

        double? lat = null;
        double? lng = null;

        // GeoLocation parsing - check if it's a simple lat/lng or full GeoJSON
        if (!string.IsNullOrWhiteSpace(r.GeoLocation))
        {
            try
            {
                using var doc = JsonDocument.Parse(r.GeoLocation);
                var root = doc.RootElement;

                // Try to get lat/lng (for backwards compatibility)
                if (root.TryGetProperty("lat", out var latProp))
                    lat = latProp.GetDouble();

                if (root.TryGetProperty("lng", out var lngProp))
                    lng = lngProp.GetDouble();
            }
            catch { }
        }

        var row = new ArchiveRow
        {
            ReportID     = r.ReportId,
            Title        = r.Title,
            Pilot        = r.User?.Username,
            Status       = r.Status?.StatusName,
            Category     = r.Category?.CategoryName,
            HeightInFeet = r.HeightInFeet,
            Description  = r.Description,
            Latitude     = lat,
            Longitude    = lng,
            AssignedAt   = r.AssignedAt,
            DecisionAt   = r.DecisionAt,
            CreatedAt    = r.TimestampEntry != null
                           ? r.TimestampEntry.DateCreated
                           : (DateTime?)null,
            GeometryGeoJson = r.GeoLocation  // Include the complete GeoJSON
        };

        return View(row);
    }

    // NY: Validation og godkjenning/avvisning (flyttet fra AdminController)

    [Authorize(Roles = "Registar")]
    public async Task<IActionResult> ValidateReport(int reportId)
    {
        var report = await _context.Reports
            .Include(r => r.User)
            .Include(r => r.Status)
            .Include(r => r.Category)
            .FirstOrDefaultAsync(r => r.ReportId == reportId);

        if (report == null) return NotFound();
        return View(report);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Registar")]//Sikrer godkjenn-knappen
    public async Task<IActionResult> ApproveReport(int reportId)
    {
        var report = await _context.Reports.FindAsync(reportId);
        if (report != null)
        {
            report.StatusId = 3; // Approved
            await _context.SaveChangesAsync();
            TempData["Message"] = $"Report {reportId} has been approved!";
        }
        return RedirectToAction("ActiveReports");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Registar")] //Sikrer avvis-knappen
    public async Task<IActionResult> RejectReport(int reportId)
    {
        var report = await _context.Reports.FindAsync(reportId);
        if (report != null)
        {
            report.StatusId = 2; // Rejected
            await _context.SaveChangesAsync();
            TempData["Message"] = $"Report {reportId} has been rejected!";
        }
        return RedirectToAction("ActiveReports");
    }
   
   //Archive pilot view
   [Authorize(Roles = "Pilot")]
    public async Task<IActionResult> ArchivePilot()
    {
        
        int? userId = HttpContext.Session.GetInt32("UserId");
        
 
        var reports = await _context.Reports
            .Include(r => r.User)
            .Include(r => r.Status)
            .Include(r => r.Category)
            .Include(r => r.TimestampEntry)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.ReportId)
            .ToListAsync();

        var data = reports.Select(r =>
        {
            double? lat = null;
            double? lng = null;

            // GeoLocation parsing (fra din kode)
            if (!string.IsNullOrWhiteSpace(r.GeoLocation))
            {
                try
                {
                    using var doc = JsonDocument.Parse(r.GeoLocation);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("lat", out var latProp))
                        lat = latProp.GetDouble();

                    if (root.TryGetProperty("lng", out var lngProp))
                        lng = lngProp.GetDouble();
                }
                catch { } // Ignorer feilformatert JSON
            }

            return new ArchiveRow
            {
                ReportID     = r.ReportId,
                Title        = r.Title,
                Pilot        = r.User?.Username,
                Status       = r.Status?.StatusName,
                Category     = r.Category?.CategoryName,
                HeightInFeet = r.HeightInFeet,
                Description  = r.Description,
                Latitude     = lat,
                Longitude    = lng,
                AssignedAt   = r.AssignedAt,
                DecisionAt   = r.DecisionAt,
                CreatedAt    = r.TimestampEntry != null
                               ? r.TimestampEntry.DateCreated
                               : (DateTime?)null
            };
        }).ToList();

        return View(data);
    }
}