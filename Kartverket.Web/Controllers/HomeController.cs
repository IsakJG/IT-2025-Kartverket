using Kartverket.Web.Data;
using Kartverket.Web.Models;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Diagnostics;

namespace Kartverket.Web.Controllers;

public class HomeController : Controller
{
    private readonly KartverketDbContext _db;
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _config;

    public HomeController(
        KartverketDbContext db,
        ILogger<HomeController> logger,
        IConfiguration config)
    {
        _db = db;
        _logger = logger;
        _config = config;
    }

    public IActionResult Index() 
    {
        return View(); 
    }


    public IActionResult MainPage() 
    {
        return View(); 
    }


    public IActionResult Privacy() 
    {
        return View(); 
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)] 
    public IActionResult Error() 
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }); 
    }

    [HttpPost]
    public IActionResult SetDarkMode([FromBody] DarkModeRequest request)
    {
        HttpContext.Session.SetString("DarkMode", request.IsDarkMode.ToString().ToLower());
        return Ok();
    }

    public class DarkModeRequest
    {
        public bool IsDarkMode { get; set; }
    }

    public IActionResult PasswordChange() 
    {
        // Get the current user's ID from session (or authentication)
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            TempData["ErrorMessage"] = "User not logged in.";
            return RedirectToAction("MainPage");
        }

        var model = new ChangePasswordViewModel { Id = userId.Value };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult PasswordChange(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Find the user by ID
        var user = _db.Users.FirstOrDefault(u => u.UserId == model.Id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("MainPage");
        }

        // Update password hash
        user.PasswordHash = Kartverket.Web.Services.PasswordHasher.HashPassword(model.Password);
        _db.Users.Update(user);
        _db.SaveChanges();

        TempData["SuccessMessage"] = "Password updated successfully.";
        return RedirectToAction("MainPage");
    }
}

