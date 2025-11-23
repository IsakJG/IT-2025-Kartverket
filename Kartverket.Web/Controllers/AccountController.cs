using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Antiforgery;
using Kartverket.Web.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace Kartverket.Web.Controllers
{
    public class AccountController : Controller
    {
        // Disse brukes for sikkerhet og logging
        private readonly IAntiforgery _antiforgery; //IAntiforegery beskytter mot at en "ond nettside" prøver å sende skjema ved å genere tokens.
        private readonly ILogger<AccountController> _logger;

        // Liste med test-brukere (i virkeligheten skal dette være i database)
        private readonly List<User> _users = new()
        {
            new User { Username = "pilot", Password = "pilot123", Role = "Pilot" },
            new User { Username = "registrar", Password = "registrar123", Role = "Registrar" }
        };

        // Dette kalles når kontrolleren opprettes
        public AccountController(IAntiforgery antiforgery, ILogger<AccountController> logger)
        {
            _antiforgery = antiforgery;
            _logger = logger;
        }

        // Viser innloggingssiden når brukeren går til /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Håndterer når brukeren trykker "Logg inn"-knappen
        [HttpPost]
        [ValidateAntiForgeryToken] // Beskytter mot hack-angrep
        public async Task<IActionResult> Login(LoginModel model)
        {
            // Sjekker om brukernavn og passord er fylt ut
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Ser etter brukeren i listen
            var user = _users.FirstOrDefault(u => 
                u.Username == model.Username && u.Password == model.Password);

            // Hvis brukeren ikke finnes eller passord er feil
            if (user == null)
            {
                ModelState.AddModelError("", "Brukernavn eller passord er feil");
                return View(model);
            }

            // Lager informasjon om brukeren (kalt "claims")
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username), // Brukerens navn
                new Claim(ClaimTypes.Role, user.Role)      // Brukerens rolle
            };

            // Lager en identitet for brukeren
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            
            // Setter egenskaper for innlogging
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false // Blir ikke lagret lenge
            };

            // Logger inn brukeren og lager en cookie
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Skriver i loggen at noen logget inn
            _logger.LogInformation("Bruker {Username} logget inn som {Role}", user.Username, user.Role);

            // Sender brukeren til riktig side basert på rolle
            if (user.Role == "Pilot")
            {
                return RedirectToAction("MainPage", "Pilot");
            }
            else if (user.Role == "Registrar")
            {
                return RedirectToAction("MainPage", "Registrar");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // Håndterer utlogging
        [HttpPost]
        [ValidateAntiForgeryToken] // Sikkerhet for utlogging også
        public async Task<IActionResult> Logout()
        {
            // Sletter innloggings-cookien
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            // Logger at noen logget ut
            _logger.LogInformation("Bruker logget ut");
            
            // Sender brukeren tilbake til hovedsiden
            return RedirectToAction("Index", "Home");
        }

        // Viser "tilgang nektet"-siden
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}