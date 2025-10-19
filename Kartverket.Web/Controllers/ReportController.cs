using Microsoft.AspNetCore.Mvc;

public class ReportController : Controller
{
    public IActionResult Archive()
    {
        return View(Reports);
    }

    private static readonly List<dynamic> Reports = new()
    {
        new { Id = 1, Title = "Mountain top", Description = "Terrain that is up into the flight path", HeightFeet = 1400, Latitude = 58.1467, Longitude = 8.0089 },
        new { Id = 2, Title = "Wind turbines", Description = "A wind turbine in the way of the path", HeightFeet = 300, Latitude = 58.1500, Longitude = 8.0100 },
        new { Id = 3, Title = "High voltage line", Description = "Tall structure in the way", HeightFeet = 200, Latitude = 58.1550, Longitude = 8.0150 }
    };

    public IActionResult Detail(int id)
    {
        var report = Reports.FirstOrDefault(r => r.Id == id);
        if (report == null)
        {
            return NotFound();
        }

        return View(report);
    }
}
