using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Kartverket.Web.Models;
using MySqlConnector;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Kartverket.Web.Data;

namespace Kartverket.Web.Controllers;

[Authorize(Roles = "Registar")]
public class RegistrarController : Controller
{
    private readonly KartverketDbContext _db;

    public RegistrarController(KartverketDbContext db)
    {
        _db = db;
    }
    public ActionResult RegisterMetode()
    {
        return View("MainPageReg");
    }
    public IActionResult PasswordChange()
    {
        // Get the current user's ID from session (or authentication)
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            TempData["ErrorMessage"] = "User not logged in.";
            return RedirectToAction("RegisterMetode");
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
            return RedirectToAction("RegisterMetode");
        }

        // Update password hash
        user.PasswordHash = Kartverket.Web.Services.PasswordHasher.HashPassword(model.Password);
        _db.Users.Update(user);
        _db.SaveChanges();

        TempData["SuccessMessage"] = "Password updated successfully.";
        return RedirectToAction("RegisterMetode");
    }
}
