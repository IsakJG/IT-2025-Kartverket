using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Kartverket.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IConfiguration _config;

        public AdminController(ILogger<AdminController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public IActionResult Index()
        {
            return View();
        }

        // NY: Validation Page for en spesifikk rapport
        public IActionResult ValidateReport(int reportId)
        {
            // Midlertidig dummy data - erstattes med database-kall senere
            var report = new 
            {
                ReportId = reportId,
                Position = GetDummyPosition(reportId),
                Height = GetDummyHeight(reportId),
                CreatedBy = GetDummyUser(reportId),
                Date = GetDummyDate(reportId),
                Description = GetDummyDescription(reportId),
                ObstacleType = GetDummyType(reportId)
            };
            
            return View(report);
        }

        // NY: Godkjenn rapport
        [HttpPost]
        public IActionResult ApproveReport(int reportId)
        {
            // Her skal du koble til database senere
            TempData["Message"] = $"Report {reportId} has been approved!";
            return RedirectToAction("Index");
        }

        // NY: Avvis rapport
        [HttpPost]
        public IActionResult RejectReport(int reportId)
        {
            // Her skal du koble til database senere
            TempData["Message"] = $"Report {reportId} has been rejected!";
            return RedirectToAction("Index");
        }

        // Hjelpemetoder for dummy data
        private string GetDummyPosition(int reportId) => reportId switch
        {
            1001 => "59.9139, 10.7522",
            1002 => "60.3913, 5.3221", 
            1003 => "63.4305, 10.3951",
            1004 => "58.9690, 5.7331",
            1005 => "61.2461, 10.4209",
            1006 => "59.9115, 10.7579",
            _ => "59.9139, 10.7522"
        };

        private string GetDummyHeight(int reportId) => reportId switch
        {
            1001 => "12.3 m",
            1002 => "5.0 m",
            1003 => "23.7 m", 
            1004 => "0.8 m",
            1005 => "7.5 m",
            1006 => "15.2 m",
            _ => "10.0 m"
        };

        private string GetDummyUser(int reportId) => reportId switch
        {
            1001 => "Ingrid Hansen",
            1002 => "Ola Nordmann",
            1003 => "Maria Berg",
            1004 => "Karl Olsen", 
            1005 => "Eva Larsen",
            1006 => "Per Johansen",
            _ => "Unknown User"
        };

        private string GetDummyDate(int reportId) => reportId switch
        {
            1001 => "2025-10-20 09:15",
            1002 => "2025-10-21 14:42",
            1003 => "2025-10-22 11:03",
            1004 => "2025-10-23 16:27",
            1005 => "2025-10-24 08:50", 
            1006 => "2025-10-25 19:05",
            _ => "2025-10-20 00:00"
        };

        private string GetDummyDescription(int reportId) => reportId switch
        {
            1001 => "Tall building near the city center with potential flight path interference during low visibility conditions.",
            1002 => "Small communication tower in residential area. Requires monitoring for future urban development.",
            1003 => "Large industrial chimney with significant height. Already marked on most aviation maps.",
            1004 => "Low obstacle - temporary construction site. Expected to be removed within 2 months.",
            1005 => "Medium height antenna array. Regular maintenance required.",
            1006 => "Office building with glass facade. Reflectivity may cause issues in certain weather conditions.",
            _ => "Standard obstacle report requiring review."
        };

        private string GetDummyType(int reportId) => reportId switch
        {
            1001 => "Building",
            1002 => "Communication Tower", 
            1003 => "Industrial Chimney",
            1004 => "Construction Site",
            1005 => "Antenna Array",
            1006 => "Office Building",
            _ => "Unknown Type"
        };
    }
}