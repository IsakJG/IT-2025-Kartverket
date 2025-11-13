using Kartverket.Web.Models;
using Microsoft.AspNetCore.Mvc;

public class AdminPartController : Controller
{
    public IActionResult Index()
    {
        // Hent brukere
        var users = new List<UserAdminViewModel>
        {
            new UserAdminViewModel { Id = 1, Name = "Test User", UserName = "testuser", Role = "Admin" }
        };

        return View("User-management", users);
    }
}