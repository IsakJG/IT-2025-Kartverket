using Kartverket.Web.Data;
using Kartverket.Web.Models;
using Kartverket.Web.Models.Entities;
using Kartverket.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Kartverket.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminPartController : Controller
{
    private readonly KartverketDbContext _db;

    public AdminPartController(KartverketDbContext db)
    {
        _db = db;
    }

    // --- USER MANAGEMENT ---

    // INDEX — LIST USERS
    public IActionResult Index()
    {
        // Henter brukere + Rolle + Organisasjon (Left Join)
        var users = (from u in _db.Users
                     join ur in _db.UserRoles on u.UserId equals ur.UserId into urj
                     from ur in urj.DefaultIfEmpty()
                     join r in _db.Roles on ur.RoleId equals r.RoleId into rj
                     from r in rj.DefaultIfEmpty()
                     join o in _db.Organization on u.OrgId equals o.OrgId into oj
                     from o in oj.DefaultIfEmpty()
                     select new UserAdminViewModel
                     {
                         Id = u.UserId,
                         Name = u.Username,
                         UserName = u.Username,
                         Role = r != null ? r.RoleName : "No role",
                         OrganizationName = o != null ? o.OrgName : "No Org"
                     }).ToList();

        return View("User-management", users);
    }

    // CREATE USER — GET
    public IActionResult Create()
    {
        ViewBag.Roles = new SelectList(_db.Roles, "RoleId", "RoleName");
        ViewBag.Organizations = new SelectList(_db.Organization, "OrgId", "OrgName");
        return View("CreateNewUser");
    }

    // CREATE USER — POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(UserAdminViewModel model)
    {
        // Validering av RoleId og OrgId manuelt hvis modellen feiler
        if (!model.RoleId.HasValue)
            ModelState.AddModelError(nameof(model.RoleId), "Please select a role.");
        
        if (!model.OrgId.HasValue)
            ModelState.AddModelError(nameof(model.OrgId), "Please select an organization.");

        if (!ModelState.IsValid)
        {
            ViewBag.Roles = new SelectList(_db.Roles, "RoleId", "RoleName");
            ViewBag.Organizations = new SelectList(_db.Organization, "OrgId", "OrgName");
            return View("CreateNewUser", model);
        }

        var user = new Kartverket.Web.Models.Entities.User
        {
            Username = model.UserName,
            Email = model.Email,
            OrgId = model.OrgId.Value, // Setter valgt organisasjon
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
        return RedirectToAction("Index");
    }

    // EDIT USER — GET
    public IActionResult Edit(int id)
    {
        var user = _db.Users.FirstOrDefault(u => u.UserId == id);
        if (user == null) return NotFound();

        var existingRole = _db.UserRoles.FirstOrDefault(ur => ur.UserId == id);

        var model = new UserAdminViewModel
        {
            Id = user.UserId,
            Name = user.Username,
            UserName = user.Username,
            Email = user.Email,
            RoleId = existingRole?.RoleId,
            OrgId = user.OrgId // Henter eksisterende org
        };

        ViewBag.Roles = new SelectList(_db.Roles, "RoleId", "RoleName", model.RoleId);
        ViewBag.Organizations = new SelectList(_db.Organization, "OrgId", "OrgName", model.OrgId);
        
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
            ViewBag.Organizations = new SelectList(_db.Organization, "OrgId", "OrgName", model.OrgId);
            return View("EditUser", model);
        }

        var user = _db.Users.FirstOrDefault(u => u.UserId == model.Id);
        if (user == null) return NotFound();

        // Oppdater felter
        user.Username = model.UserName;
        user.Email = model.Email;
        
        if (model.OrgId.HasValue)
        {
            user.OrgId = model.OrgId.Value;
        }

        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            user.PasswordHash = PasswordHasher.HashPassword(model.Password);
        }

        _db.Users.Update(user);

        // Oppdater rolle
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
        return RedirectToAction("Index");
    }

    // DELETE USER - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        try
        {
            var user = _db.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Index");
            }

            var userRoles = _db.UserRoles.Where(ur => ur.UserId == id);
            _db.UserRoles.RemoveRange(userRoles);

            _db.Users.Remove(user);
            _db.SaveChanges();

            TempData["SuccessMessage"] = $"User {user.Username} was successfully deleted.";
        }
        catch (DbUpdateException)
        {
            TempData["ErrorMessage"] = "Cannot delete user because they have related records (e.g. reports).";
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "An error occurred while deleting the user.";
        }

        return RedirectToAction("Index");
    }

    // --- ORGANIZATION MANAGEMENT ---

    // GET: Vis liste over alle organisasjoner og antall brukere
    public IActionResult ManageOrganizations()
    {
        var orgs = _db.Organization
            .Include(o => o.Users)
            .Select(o => new OrganizationListViewModel 
            {
                OrgId = o.OrgId,
                OrgName = o.OrgName,
                UserCount = o.Users.Count
            })
            .ToList();

        return View(orgs);
    }

    // GET: Create Organization
    public IActionResult CreateOrganization()
    {
        return View();
    }

    // POST: Create Organization
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateOrganization(CreateOrgViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Sjekk om navnet finnes
            if (_db.Organization.Any(o => o.OrgName.ToLower() == model.OrgName.ToLower()))
            {
                ModelState.AddModelError("OrgName", "Organization already exists.");
                return View(model);
            }

            var org = new Organization
            {
                OrgName = model.OrgName
            };

            _db.Organization.Add(org);
            _db.SaveChanges();

            TempData["SuccessMessage"] = $"Organization '{model.OrgName}' created!";
            
            // Endret: Går nå tilbake til ManageOrganizations i stedet for Index
            return RedirectToAction("ManageOrganizations");
        }
        return View(model);
    }

    // POST: Delete Organization
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteOrganization(int id)
    {
        try 
        {
            var org = _db.Organization
                .Include(o => o.Users)
                .FirstOrDefault(o => o.OrgId == id);

            if (org == null) 
            {
                TempData["ErrorMessage"] = "Organization not found.";
                return RedirectToAction("ManageOrganizations");
            }

            // Sjekk om organisasjonen har brukere
            if (org.Users.Any())
            {
                TempData["ErrorMessage"] = $"Cannot delete '{org.OrgName}' because {org.Users.Count} users are assigned to it. Please reassign the users first.";
                return RedirectToAction("ManageOrganizations");
            }

            _db.Organization.Remove(org);
            _db.SaveChanges();

            TempData["SuccessMessage"] = $"Organization '{org.OrgName}' was deleted.";
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "An error occurred while deleting the organization.";
        }

        return RedirectToAction("ManageOrganizations");
    }
}