using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kartverket.Web.Data;
using Kartverket.Web.Models; // Antar ActiveReportRow og ArchiveRow ligger her
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace Kartverket.Web.Controllers
{
    /// <summary>
    /// Kontroller for håndtering av rapporter (innsendte hindre).
    /// Håndterer arbeidsflyt for godkjenning (Registar) og visning av arkiv (Pilot/Admin).
    /// </summary>
    [Authorize]
    public class ReportController : Controller 
    {
        private readonly KartverketDbContext _context;
        private readonly ILogger<ReportController> _logger;

        // Konstanter for statuser og roller
        private const int StatusPending = 1;
        private const int StatusRejected = 2;
        private const int StatusApproved = 3;
        private const string RoleRegistar = "Registar"; // Legacy skrivemåte
        private const string RoleAdmin = "Admin";
        private const string RolePilot = "Pilot";

        public ReportController(KartverketDbContext context, ILogger<ReportController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // --- REGISTAR FUNCTIONS ---

        /// <summary>
        /// Henter liste over aktive rapporter som venter på godkjenning.
        /// </summary>
        [Authorize(Roles = RoleRegistar)]
        public async Task<IActionResult> ActiveReports()
        {
            var reports = await _context.Reports
                .AsNoTracking() // Optimalisering for skrivebeskyttet visning
                .Include(r => r.User)
                .Include(r => r.Status)
                .Include(r => r.TimestampEntry)
                .Where(r => r.StatusId == StatusPending)
                .OrderBy(r => r.ReportId)
                .ToListAsync();

            var rows = reports.Select(r =>
            {
                // Bruker hjelpemetode for å unngå duplisert logikk
                var (lat, lng) = ParseGeoLocation(r.GeoLocation);

                string posText;
                if (lat.HasValue && lng.HasValue)
                    posText = $"{lat.Value:F5}, {lng.Value:F5}";
                else
                    posText = r.GeoLocation ?? "-";

                return new ActiveReportRow
                {
                    ReportId  = r.ReportId,
                    CreatedBy = r.User?.Username ?? "Unknown",
                    Status    = r.Status?.StatusName ?? "Unknown",
                    CreatedAt = r.TimestampEntry?.DateCreated,
                    Height    = $"{r.HeightInFeet} ft",
                    Position  = posText
                };
            }).ToList();

            return View(rows);
        }

        /// <summary>
        /// Viser detaljer for en spesifikk rapport for validering.
        /// </summary>
        [Authorize(Roles = RoleRegistar)]
        public async Task<IActionResult> ValidateReport(int reportId)
        {
            var report = await _context.Reports
                .AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Status)
                .Include(r => r.Category)
                .FirstOrDefaultAsync(r => r.ReportId == reportId);

            if (report == null) return NotFound();
            var (positionText, lat, lng) = ParsePosition(report.GeoLocation);

    // Sender verdiene videre via ViewBag (enkelt siden modellen er Report)
    ViewBag.PositionText = positionText;
    ViewBag.Latitude     = lat;
    ViewBag.Longitude    = lng;
            
            return View(report);
        }

        /// <summary>
        /// Godkjenner en rapport.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleRegistar)]
        public async Task<IActionResult> ApproveReport(int reportId)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report != null)
            {
                report.StatusId = StatusApproved;
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Rapport {ReportId} ble godkjent av Registar.", reportId);
                TempData["Message"] = $"Report {reportId} has been approved!";
            }
            return RedirectToAction(nameof(ActiveReports));
        }

        /// <summary>
        /// Avviser en rapport.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleRegistar)]
        public async Task<IActionResult> RejectReport(int reportId)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report != null)
            {
                report.StatusId = StatusRejected;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Rapport {ReportId} ble avvist av Registar.", reportId);
                TempData["Message"] = $"Report {reportId} has been rejected!";
            }
            return RedirectToAction(nameof(ActiveReports));
        }

        // --- ARCHIVE & DETAILS ---

        /// <summary>
        /// Viser hele arkivet. Kun tilgjengelig for Registar og Admin.
        /// </summary>
        [Authorize(Roles = $"{RoleRegistar}, {RoleAdmin}")]
        public async Task<IActionResult> Archive()
        {
            var reports = await _context.Reports
                .AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Status)
                .Include(r => r.Category)
                .Include(r => r.TimestampEntry)
                .OrderByDescending(r => r.ReportId)
                .ToListAsync();

            var data = reports.Select(MapToArchiveRow).ToList();

            return View(data);
        }

        /// <summary>
        /// Viser arkiv for innlogget Pilot (kun egne rapporter).
        /// </summary>
        [Authorize(Roles = RolePilot)]
        public async Task<IActionResult> ArchivePilot()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            
            if (userId == null) return RedirectToAction("Login", "Auth");

            var reports = await _context.Reports
                .AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Status)
                .Include(r => r.Category)
                .Include(r => r.TimestampEntry)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ReportId)
                .ToListAsync();

            var data = reports.Select(MapToArchiveRow).ToList();

            return View(data);
        }

        /// <summary>
        /// Viser detaljert visning av en rapport.
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            var r = await _context.Reports
                .AsNoTracking()
                .Include(rep => rep.User)
                .Include(rep => rep.Status)
                .Include(rep => rep.Category)
                .Include(rep => rep.TimestampEntry)
            
                .FirstOrDefaultAsync(rep => rep.ReportId == id);

            if (r == null) return NotFound();

            var row = MapToArchiveRow(r);

            var (positionText, lat, lng) = ParsePosition(r.GeoLocation);

            row.PositionText = positionText;
            row.Latitude     = lat;
            row.Longitude    = lng;
            // Inkluderer full GeoJSON for Details-visning
            row.GeometryGeoJson = r.GeoLocation; 

            return View(row);
        }

        // --- HELPER METHODS (Private) ---

        /// <summary>
        /// Mapper fra Database-entitet til ViewModel for Arkiv-visning.
        /// </summary>
        private ArchiveRow MapToArchiveRow(Kartverket.Web.Models.Entities.Report r)
        {
            var (positionText, lat, lng) = ParsePosition(r.GeoLocation);

            return new ArchiveRow
            {
                ReportId     = r.ReportId,
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
                CreatedAt    = r.TimestampEntry?.DateCreated,
                PositionText = positionText
            };
        }

        /// <summary>
        /// Forsøker å parse lat/lng fra en GeoJSON-streng eller enkel JSON.
        /// Returnerer (null, null) hvis parsing feiler.
        /// </summary>
        private (double? Lat, double? Lng) ParseGeoLocation(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return (null, null);

            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                
                double? lat = null;
                double? lng = null;

                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("lat", out var latProp) && latProp.ValueKind == JsonValueKind.Number)
                        lat = latProp.GetDouble();

                    if (root.TryGetProperty("lng", out var lngProp) && lngProp.ValueKind == JsonValueKind.Number)
                        lng = lngProp.GetDouble();
                }

                return (lat, lng);
            }
            catch
            {
                // Logging av parsing-feil kan legges til her ved behov
                return (null, null);
            }
        }

        private (string display, double? lat, double? lng) ParsePosition(string? json)
{
    if (string.IsNullOrWhiteSpace(json))
        return ("N/A", null, null);

    try
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // ---------- CASE 1: Enkel JSON: { "lat": X, "lng": Y } ----------
        if (root.ValueKind == JsonValueKind.Object &&
            root.TryGetProperty("lat", out var latProp) &&
            root.TryGetProperty("lng", out var lngProp))
        {
            if (TryReadDouble(latProp, out var latVal) &&
                TryReadDouble(lngProp, out var lngVal))
            {
                return ($"{latVal:F5}, {lngVal:F5}", latVal, lngVal);
            }
        }

        // ---------- CASE 2: FeatureCollection → gå ned til geometry ----------
        if (root.ValueKind == JsonValueKind.Object &&
            root.TryGetProperty("type", out var typeProp) &&
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

        // ---------- CASE 3: GeoJSON Point ----------
        if (root.ValueKind == JsonValueKind.Object &&
            root.TryGetProperty("type", out var geomTypeProp) &&
            geomTypeProp.ValueKind == JsonValueKind.String &&
            geomTypeProp.GetString() == "Point" &&
            root.TryGetProperty("coordinates", out var pointCoords) &&
            pointCoords.ValueKind == JsonValueKind.Array)
        {
            var values = pointCoords.EnumerateArray().ToList();
            if (values.Count >= 2 &&
                values[0].ValueKind == JsonValueKind.Number &&
                values[1].ValueKind == JsonValueKind.Number)
            {
                // GeoJSON: [lon, lat]
                var lngVal = values[0].GetDouble();
                var latVal = values[1].GetDouble();

                return ($"{latVal:F5}, {lngVal:F5}", latVal, lngVal);
            }
        }

        // ---------- CASE 4: GeoJSON LineString (start → slutt) ----------
        if (root.ValueKind == JsonValueKind.Object &&
            root.TryGetProperty("type", out var lineTypeProp) &&
            lineTypeProp.ValueKind == JsonValueKind.String &&
            lineTypeProp.GetString() == "LineString" &&
            root.TryGetProperty("coordinates", out var coords) &&
            coords.ValueKind == JsonValueKind.Array)
        {
            var list = coords.EnumerateArray().ToList();

            if (list.Count >= 1)
            {
                // Startpunkt
                var first = list.First().EnumerateArray().ToList();
                if (first.Count >= 2 &&
                    first[0].ValueKind == JsonValueKind.Number &&
                    first[1].ValueKind == JsonValueKind.Number)
                {
                    var sLng = first[0].GetDouble();
                    var sLat = first[1].GetDouble();

                    // Hvis vi har sluttpunkt → vis med pil
                    if (list.Count >= 2)
                    {
                        var last = list.Last().EnumerateArray().ToList();
                        if (last.Count >= 2 &&
                            last[0].ValueKind == JsonValueKind.Number &&
                            last[1].ValueKind == JsonValueKind.Number)
                        {
                            var eLng = last[0].GetDouble();
                            var eLat = last[1].GetDouble();

                            string display = $"{sLat:F5}, {sLng:F5} → {eLat:F5}, {eLng:F5}";
                            return (display, sLat, sLng);
                        }
                    }

                    // Hvis bare ett punkt i linjen → behandle som punkt
                    return ($"{sLat:F5}, {sLng:F5}", sLat, sLng);
                }
            }
        }

        // Ingenting traff → ingen gyldig posisjon
        return ("N/A", null, null);
    }
    catch
    {
        return ("N/A", null, null);
    }

    // Lokal hjelpemetode for å tolke tall som både number og string
    static bool TryReadDouble(JsonElement element, out double value)
    {
        if (element.ValueKind == JsonValueKind.Number)
        {
            value = element.GetDouble();
            return true;
        }

        if (element.ValueKind == JsonValueKind.String &&
            double.TryParse(element.GetString(), System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out var parsed))
        {
            value = parsed;
            return true;
        }

        value = 0;
        return false;
    }
}

    }
}