using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kartverket.Web.Data;
using Kartverket.Web.Models;
using Kartverket.Web.Models.Entities;
using Kartverket.Web.Services;

namespace Kartverket.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly KartverketDbContext _context;
        private readonly ILogger<AuthController> _logger;

        public AuthController(KartverketDbContext context, ILogger<AuthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Registreringsskjema
        public IActionResult Register()
        {
            return View();
        }

        // POST: Behandle registrering
        [HttpPost]
        [ValidateAntiForgeryToken] //Legger til beskyttelse mot CSRF på alle POST-forespørsler
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Sjekk om brukernavn eksisterer
                    if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                    {
                        ModelState.AddModelError("Username", "Brukernavn er allerede i bruk");
                        return View(model);
                    }

                    // Sjekk om e-post eksisterer
                    if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                    {
                        ModelState.AddModelError("Email", "E-post er allerede i bruk");
                        return View(model);
                    }

                    // Finn neste UserId
                    var maxUserId = await _context.Users.MaxAsync(u => (int?)u.UserId) ?? 0;
                    var nextUserId = maxUserId + 1;

                    // Lag ny bruker med hashet passord
                    var user = new User
                    {
                        UserId = nextUserId,
                        Username = model.Username,
                        Email = model.Email,
                        PasswordHash = PasswordHasher.HashPassword(model.Password),
                        OrgId = model.OrgId,
                        CreatedAt = DateTime.Now
                    };

                    // Legg til Pilot-rolle som standard (RoleId = 3)
                    var userRole = new UserRole
                    {
                        UserId = user.UserId,
                        RoleId = 3 // Pilot
                    };

                    // Lagre i database
                    _context.Users.Add(user);
                    _context.UserRoles.Add(userRole);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Ny bruker registrert: {Username} (ID: {UserId})", user.Username, user.UserId);
                    
                    TempData["SuccessMessage"] = "Bruker registrert vellykket! Du kan nå logge inn.";
                    return RedirectToAction("Login", "Auth");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Feil under registrering av bruker {Username}", model.Username);
                    ModelState.AddModelError("", "En feil oppstod under registrering. Vennligst prøv igjen.");
                }
            }

            return View(model);
        }

        // GET: Login skjema
        public IActionResult Login()
        {
            return View();
        }

        // POST: Behandle login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LogIn model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Finn bruker med roller
                    var user = await _context.Users
                        .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                        .Include(u => u.Organization)
                        .FirstOrDefaultAsync(u => u.Username == model.Username);

                    if (user != null && PasswordHasher.VerifyPassword(model.Password, user.PasswordHash))
                    {
                        // Login vellykket - lagre session
                        HttpContext.Session.SetInt32("UserId", user.UserId);
                        HttpContext.Session.SetString("Username", user.Username);
                        HttpContext.Session.SetString("Email", user.Email);
                        HttpContext.Session.SetString("Organization", user.Organization?.OrgName ?? "Ukjent");
                        
                        // Lagre roller som komma-separert liste
                        var roles = user.UserRoles.Select(ur => ur.Role.RoleName).ToList();
                        HttpContext.Session.SetString("UserRoles", string.Join(",", roles));

                        _logger.LogInformation("Bruker logget inn: {Username} (Roller: {Roles})", 
                            user.Username, string.Join(", ", roles));

                        // Redirect basert på roller
                        if (roles.Contains("Admin"))
                            return RedirectToAction("Index", "AdminPart");
                        else if (roles.Contains("Registrar") || roles.Contains("Registar")) 
                            return RedirectToAction("RegisterMetode", "Registrar");
                        else
                            return RedirectToAction("MainPage", "Home");
                    }

                    ModelState.AddModelError("", "Feil brukernavn eller passord");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Feil under login for bruker {Username}", model.Username);
                    ModelState.AddModelError("", "En feil oppstod under innlogging. Vennligst prøv igjen.");
                }
            }

            return View(model);
        }

        // Logout
        public IActionResult Logout()
        {
            var username = HttpContext.Session.GetString("Username");
            HttpContext.Session.Clear();
            _logger.LogInformation("Bruker logget ut: {Username}", username);
            
            TempData["SuccessMessage"] = "Du er nå logget ut.";
            return RedirectToAction("Index", "Home");
        }
    }
}