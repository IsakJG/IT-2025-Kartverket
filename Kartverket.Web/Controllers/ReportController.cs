using Microsoft.AspNetCore.Mvc;

public class ReportController : Controller // Changed from ObstacleController to ReportController
{
    public IActionResult Archive() // Changed from DataForm to Archive
    {
        return View(Reports); // Changed from returning empty view to returning list of reports
    }

    private static readonly List<dynamic> Reports = new() // Changed from Obstacles to Reports
    {
        new { Id = 1, Title = "Mountain top", Description = "Terrain that is up into the flight path", HeightFeet = 1400, Latitude = 58.1467, Longitude = 8.0089 }, // Changed from ObstacleName to Title
        new { Id = 2, Title = "Wind turbines", Description = "A wind turbine in the way of the path", HeightFeet = 300, Latitude = 58.1500, Longitude = 8.0100 }, // Changed from ObstacleName to Title
        new { Id = 3, Title = "High voltage line", Description = "Tall structure in the way", HeightFeet = 200, Latitude = 58.1550, Longitude = 8.0150 } // Changed from ObstacleName to Title
    };

    public IActionResult Details(int id) // New action method to view details of a specific report
    {
        var report = Reports.FirstOrDefault(r => r.Id == id); // Changed from obstacle to report
        if (report == null) // Changed from obstacle to report
        {
            return NotFound(); // Changed from returning empty view to returning NotFound if report not found
        }

        return View(report); // Changed from returning empty view to returning the specific report
    }
}
