using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kartverket.Web.Data;
using Kartverket.Web.Models;
using Kartverket.Web.Services;
// Alias for å skille mellom ViewModel User og Database Entity User hvis navnene kolliderer
using UserEntity = Kartverket.Web.Models.Entities.User;
using UserRoleEntity = Kartverket.Web.Models.Entities.UserRole;

namespace Kartverket.Web.Controllers
{
    /// <summary>
    /// Håndterer autentisering mot databasen, registrering av nye brukere og sesjonshåndtering.
    /// </summary>
    public class AuthController : Controller
    {
        private readonly KartverketDbContext _context;
        private readonly ILogger<AuthController> _logger;

        // Konstanter for Roller og Session-nøkler (Best Practice for å unngå skrivefeil)
        private const int DefaultRoleId = 3; // Pilot
        private const string RoleAdmin = "Admin";
        private const string RoleRegistrar = "Registar";       
        private const string SessionKeyUserId = "UserId";
        private const string SessionKeyUsername = "Username";
        private const string SessionKeyEmail = "Email";
        private const string SessionKeyOrg = "Organization";
        private const string SessionKeyRoles = "UserRoles";

        /// <summary>
        /// Initialiserer AuthController med databasekontekst og logger.
        /// </summary>
        public AuthController(KartverketDbContext context, ILogger<AuthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Viser innloggingsskjemaet.
        /// </summary>
        [HttpGet]
        public IActionResult Login()
        {
            // Hvis brukeren allerede er logget inn, send dem til hovedsiden
            if (HttpContext.Session.GetString(SessionKeyUserId) != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        /// <summary>
        /// Validerer legitimasjon og oppretter brukersesjon.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LogIn model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Hent bruker inkludert roller og organisasjon for å minimere antall databasekall senere
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .Include(u => u.Organization)
                    .FirstOrDefaultAsync(u => u.Username == model.Username);

                // Verifiser passord
                if (user != null && PasswordHasher.VerifyPassword(model.Password, user.PasswordHash))
                {
                    // Opprett sesjonsdata
                    HttpContext.Session.SetInt32(SessionKeyUserId, user.UserId);
                    HttpContext.Session.SetString(SessionKeyUsername, user.Username);
                    HttpContext.Session.SetString(SessionKeyEmail, user.Email);
                    HttpContext.Session.SetString(SessionKeyOrg, user.Organization?.OrgName ?? "Ukjent");
                    
                    var roles = user.UserRoles.Select(ur => ur.Role.RoleName).ToList();
                    HttpContext.Session.SetString(SessionKeyRoles, string.Join(",", roles));

                    _logger.LogInformation("Bruker {Username} logget inn. Roller: {Roles}", 
                        user.Username, string.Join(", ", roles));

                    return RedirectBasedOnRole(roles);
                }

                _logger.LogWarning("Mislykket innlogging for bruker: {Username}", model.Username);
                ModelState.AddModelError(string.Empty, "Wrong username or password.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Databasefeil under innlogging for {Username}", model.Username);
                ModelState.AddModelError(string.Empty, "Could not log in due to a technical error.");
            }

            return View(model);
        }

        /// <summary>
        /// Logger ut brukeren og tømmer sesjonen.
        /// </summary>
        public IActionResult Logout()
        {
            var username = HttpContext.Session.GetString(SessionKeyUsername);
            
            HttpContext.Session.Clear(); // Sletter all data i sesjonen
            
            _logger.LogInformation("Bruker logget ut: {Username}", username ?? "Ukjent");
            
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Hjelpemetode for å rute brukeren til riktig startside basert på rolle.
        /// </summary>
        private IActionResult RedirectBasedOnRole(List<string> roles)
        {
            if (roles.Contains(RoleAdmin))
            {
                return RedirectToAction("Index", "AdminPart");
            }
            
            if (roles.Contains(RoleRegistrar))
            {
                return RedirectToAction("RegisterMetode", "Registrar");
            }

            // Standard fallback
            return RedirectToAction("MainPage", "Home");
        }
    }
}