using Kartverket.Web.Data;
using Kartverket.Web.Models;
using Kartverket.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using UserEntity = Kartverket.Web.Models.Entities.User;
using OrganizationEntity = Kartverket.Web.Models.Entities.Organization;
using UserRoleEntity = Kartverket.Web.Models.Entities.UserRole;

namespace Kartverket.Web.Controllers;

/// <summary>
/// Kontroller for administratorfunksjoner. Håndterer administrasjon av brukere og organisasjoner.
/// Krever rollen "Admin" for tilgang.
/// </summary>
[Authorize(Roles = "Admin")]
public class AdminPartController : Controller
{
    private readonly KartverketDbContext _db;
    private readonly ILogger<AdminPartController> _logger;

    public AdminPartController(KartverketDbContext db, ILogger<AdminPartController> logger)
    {
        _db = db;
        _logger = logger;
    }

    #region User Management

    /// <summary>
    /// Viser liste over alle brukere med tilhørende rolle og organisasjon.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Bruker Method Syntax med Include for å hente relaterte data effektivt (Eager Loading)
        // AsNoTracking brukes for leseoperasjoner for bedre ytelse.
        var users = await _db.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.Organization)
            .Select(u => new UserAdminViewModel
            {
                Id = u.UserId,
                Name = u.Username,
                UserName = u.Username,
                // Henter første rollenavn hvis det finnes, ellers "No role"
                Role = u.UserRoles.FirstOrDefault() != null ? u.UserRoles.First().Role.RoleName : "No role",
                OrganizationName = u.Organization != null ? u.Organization.OrgName : "No Org",
                Email = u.Email
            })
            .ToListAsync();

        return View("User-management", users);
    }

    /// <summary>
    /// Viser skjema for å opprette ny bruker.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await PrepareDropdownListsAsync();
        return View("CreateNewUser");
    }

    /// <summary>
    /// Behandler opprettelse av ny bruker.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserAdminViewModel model)
    {
        if (!model.RoleId.HasValue)
            ModelState.AddModelError(nameof(model.RoleId), "Please select a role.");
        
        if (!model.OrgId.HasValue)
            ModelState.AddModelError(nameof(model.OrgId), "Please select an organization.");

        if (!ModelState.IsValid)
        {
            await PrepareDropdownListsAsync();
            return View("CreateNewUser", model);
        }

        try 
        {
            var user = new UserEntity
            {
                Username = model.UserName,
                Email = model.Email,
                OrgId = model.OrgId.Value,
                PasswordHash = PasswordHasher.HashPassword(model.Password),
                CreatedAt = DateTime.Now
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync(); // Må lagre bruker først for å få generert ID

            var userRole = new UserRoleEntity
            {
                UserId = user.UserId,
                RoleId = model.RoleId.Value
            };

            _db.UserRoles.Add(userRole);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Admin opprettet ny bruker: {Username}", user.Username);
            TempData["SuccessMessage"] = "User created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Feil under opprettelse av bruker {Username}", model.UserName);
            ModelState.AddModelError(string.Empty, "Could not create user.");
            await PrepareDropdownListsAsync();
            return View("CreateNewUser", model);
        }
    }

    /// <summary>
    /// Viser skjema for redigering av eksisterende bruker.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (user == null) return NotFound();

        var existingRole = await _db.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == id);

        var model = new UserAdminViewModel
        {
            Id = user.UserId,
            Name = user.Username,
            UserName = user.Username,
            Email = user.Email,
            RoleId = existingRole?.RoleId,
            OrgId = user.OrgId
        };

        await PrepareDropdownListsAsync(model.RoleId, model.OrgId);
        return View("EditUser", model);
    }

    /// <summary>
    /// Lagrer endringer på en eksisterende bruker.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserAdminViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PrepareDropdownListsAsync(model.RoleId, model.OrgId);
            return View("EditUser", model);
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == model.Id);
        if (user == null) return NotFound();

        // Oppdater brukerfelter
        user.Username = model.UserName;
        user.Email = model.Email;
        
        if (model.OrgId.HasValue)
            user.OrgId = model.OrgId.Value;

        if (!string.IsNullOrWhiteSpace(model.Password))
            user.PasswordHash = PasswordHasher.HashPassword(model.Password);

        _db.Users.Update(user);

        // Håndter rolleoppdatering
        if (model.RoleId.HasValue)
        {
            var userRole = await _db.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == user.UserId);
            if (userRole != null)
            {
                if (userRole.RoleId != model.RoleId.Value)
                {
                    userRole.RoleId = model.RoleId.Value;
                    _db.UserRoles.Update(userRole);
                }
            }
            else
            {
                _db.UserRoles.Add(new UserRoleEntity { UserId = user.UserId, RoleId = model.RoleId.Value });
            }
        }

        await _db.SaveChangesAsync();
        
        _logger.LogInformation("Admin oppdaterte bruker: {Username}", user.Username);
        TempData["SuccessMessage"] = "User updated successfully.";
        
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Sletter en bruker fra systemet.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            // Slett tilhørende roller først (Referential Integrity)
            var userRoles = _db.UserRoles.Where(ur => ur.UserId == id);
            _db.UserRoles.RemoveRange(userRoles);

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Admin slettet bruker ID: {UserId}", id);
            TempData["SuccessMessage"] = $"User {user.Username} was successfully deleted.";
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Forsøk på å slette bruker ID {UserId} feilet pga relasjoner.", id);
            TempData["ErrorMessage"] = "Cannot delete user because they have related records (e.g. reports).";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Uventet feil ved sletting av bruker ID {UserId}", id);
            TempData["ErrorMessage"] = "An error occurred while deleting the user.";
        }

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Organization Management

    /// <summary>
    /// Viser liste over organisasjoner og antall tilknyttede brukere.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ManageOrganizations()
    {
        var orgs = await _db.Organization
            .AsNoTracking()
            .Include(o => o.Users)
            .Select(o => new OrganizationListViewModel 
            {
                OrgId = o.OrgId,
                OrgName = o.OrgName,
                UserCount = o.Users.Count
            })
            .ToListAsync();

        return View(orgs);
    }

    /// <summary>
    /// Viser skjema for å opprette ny organisasjon.
    /// </summary>
    [HttpGet]
    public IActionResult CreateOrganization()
    {
        return View();
    }

    /// <summary>
    /// Behandler opprettelse av ny organisasjon.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOrganization(CreateOrgViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Case-insensitive sjekk
            if (await _db.Organization.AnyAsync(o => o.OrgName.ToLower() == model.OrgName.ToLower()))
            {
                ModelState.AddModelError("OrgName", "Organization already exists.");
                return View(model);
            }

            var org = new OrganizationEntity
            {
                OrgName = model.OrgName
            };

            _db.Organization.Add(org);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Ny organisasjon opprettet: {OrgName}", model.OrgName);
            TempData["SuccessMessage"] = $"Organization '{model.OrgName}' created!";
            
            return RedirectToAction(nameof(ManageOrganizations));
        }
        return View(model);
    }

    /// <summary>
    /// Sletter en organisasjon hvis den ikke har aktive brukere.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteOrganization(int id)
    {
        try 
        {
            var org = await _db.Organization
                .Include(o => o.Users)
                .FirstOrDefaultAsync(o => o.OrgId == id);

            if (org == null) 
            {
                TempData["ErrorMessage"] = "Organization not found.";
                return RedirectToAction(nameof(ManageOrganizations));
            }

            if (org.Users.Any())
            {
                TempData["ErrorMessage"] = $"Cannot delete '{org.OrgName}' because {org.Users.Count} users are assigned to it.";
                return RedirectToAction(nameof(ManageOrganizations));
            }

            _db.Organization.Remove(org);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Organisasjon slettet: {OrgName}", org.OrgName);
            TempData["SuccessMessage"] = $"Organization '{org.OrgName}' was deleted.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Feil ved sletting av organisasjon ID {Id}", id);
            TempData["ErrorMessage"] = "An error occurred while deleting the organization.";
        }

        return RedirectToAction(nameof(ManageOrganizations));
    }

    #endregion

    /// <summary>
    /// Hjelpemetode for å fylle SelectLists for Views.
    /// </summary>
    private async Task PrepareDropdownListsAsync(int? selectedRoleId = null, int? selectedOrgId = null)
    {
        var roles = await _db.Roles.ToListAsync();
        var orgs = await _db.Organization.ToListAsync();

        ViewBag.Roles = new SelectList(roles, "RoleId", "RoleName", selectedRoleId);
        ViewBag.Organizations = new SelectList(orgs, "OrgId", "OrgName", selectedOrgId);
    }
}