using Kartverket.Web.Data;
using Kartverket.Web.Models;
using Kartverket.Web.Models.Entities;
using Kartverket.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

public class AdminPartController : Controller
{
    private readonly KartverketDbContext _db;

    public AdminPartController(KartverketDbContext db)
    {
        _db = db;
    }

    // INDEX — LIST USERS (use left-joins so users without a role are still shown)
    public IActionResult Index()
    {
        var users = (from u in _db.Users
                     join ur in _db.UserRoles on u.UserId equals ur.UserId into urj
                     from ur in urj.DefaultIfEmpty()
                     join r in _db.Roles on ur.RoleId equals r.RoleId into rj
                     from r in rj.DefaultIfEmpty()
                     select new UserAdminViewModel
                     {
                         Id = u.UserId,
                         Name = u.Username,
                         UserName = u.Username,
                         Role = r != null ? r.RoleName : "No role"
                     }).ToList();

        return View("User-management", users);
    }

    // CREATE USER — GET
    public IActionResult Create()
    {
        ViewBag.Roles = new SelectList(_db.Roles, "RoleId", "RoleName");
        return View("CreateNewUser");
    }

    // CREATE USER — POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(UserAdminViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Roles = new SelectList(_db.Roles, "RoleId", "RoleName");
            return View("CreateNewUser", model);
        }

        if (!model.RoleId.HasValue)
        {
            ModelState.AddModelError(nameof(model.RoleId), "Please select a role.");
            ViewBag.Roles = new SelectList(_db.Roles, "RoleId", "RoleName");
            return View("CreateNewUser", model);
        }

        var user = new User
        {
            Username = model.UserName,
            Email = model.Email,
            OrgId = 1,
            PasswordHash = PasswordHasher.HashPassword(model.Password),
            CreatedAt = DateTime.Now
        };

        _db.Users.Add(user);
        _db.SaveChanges();

        var userRole = new UserRole
        {
            UserId = user.UserId,
            RoleId = model.RoleId.Value
        };

        _db.UserRoles.Add(userRole);
        _db.SaveChanges();

        TempData["SuccessMessage"] = "User created successfully.";

        // Redirect so the user-management list is reloaded and shows the new user
        return RedirectToAction("Index", "AdminPart");
    }

    // EDIT USER — GET
    public IActionResult Edit(int id)
    {
        var user = _db.Users.FirstOrDefault(u => u.UserId == id);
        if (user == null)
            return NotFound();

        var existingRole = _db.UserRoles.FirstOrDefault(ur => ur.UserId == id);

        var model = new UserAdminViewModel
        {
            Id = user.UserId,
            Name = user.Username,
            UserName = user.Username,
            Email = user.Email,
            RoleId = existingRole?.RoleId
        };

        ViewBag.Roles = new SelectList(_db.Roles, "RoleId", "RoleName", model.RoleId);
        return View("EditUser", model);
    }

    // EDIT USER — POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(UserAdminViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Roles = new SelectList(_db.Roles, "RoleId", "RoleName", model.RoleId);
            return View("EditUser", model);
        }

        var user = _db.Users.FirstOrDefault(u => u.UserId == model.Id);
        if (user == null)
            return NotFound();

        // Update basic fields
        user.Username = model.UserName;
        user.Email = model.Email;

        // Update password only when provided
        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            user.PasswordHash = PasswordHasher.HashPassword(model.Password);
        }

        _db.Users.Update(user);

        // Update or add role mapping (assumes single role per user)
        var userRole = _db.UserRoles.FirstOrDefault(ur => ur.UserId == user.UserId);
        if (model.RoleId.HasValue)
        {
            if (userRole != null)
            {
                userRole.RoleId = model.RoleId.Value;
                _db.UserRoles.Update(userRole);
            }
            else
            {
                _db.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = model.RoleId.Value });
            }
        }

        _db.SaveChanges();

        TempData["SuccessMessage"] = "User updated successfully.";
        return RedirectToAction("Index", "AdminPart");
    }
}
