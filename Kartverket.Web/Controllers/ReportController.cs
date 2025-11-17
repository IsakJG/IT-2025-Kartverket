using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kartverket.Web.Data;
using Kartverket.Web.Models.Entities;

public class ReportController : Controller 
{
    private readonly KartverketDbContext _context;

    public ReportController(KartverketDbContext context)
    {
        _context = context;
    }

    // NY: Active Reports fra database
    public async Task<IActionResult> ActiveReports() 
    {
        var activeReports = await _context.Reports
            .Include(r => r.User)
            .Include(r => r.Status)
            .Include(r => r.Category)
            .Where(r => r.StatusId == 1) // Pending reports
            .ToListAsync();

        return View(activeReports);
    }

    // Eksisterende: Archive med statisk data (behold denne)
    public IActionResult Archive() 
    {
        return View(Reports); 
    }

    // Eksisterende: Details med statisk data (behold denne)
    public IActionResult Details(int id)
    {
        var report = Reports.FirstOrDefault(r => r.Id == id);
        if (report == null)
        {
            return NotFound();
        }
        return View(report);
    }

    // Eksisterende statisk data (behold denne)
    private static readonly List<dynamic> Reports = new() 
    {
        new { Id = 1, Title = "Mountain top", Description = "Terrain that is up into the flight path", HeightFeet = 1400, Latitude = 58.1467, Longitude = 8.0089 }, 
        new { Id = 2, Title = "Wind turbines", Description = "A wind turbine in the way of the path", HeightFeet = 300, Latitude = 58.1500, Longitude = 8.0100 }, 
        new { Id = 3, Title = "High voltage line", Description = "Tall structure in the way", HeightFeet = 200, Latitude = 58.1550, Longitude = 8.0150 } 
    };

    // NY: Validation og godkjenning/avvisning (flyttet fra AdminController)
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
}