using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.Identity.Client;



namespace Kartverket.Web.Controllers
{
    public class AccountController : Controller // Enkel AccountController for innlogging og utlogging
    {
        [HttpGet]
        public IActionResult Login() // Viser innloggingssiden
        {
            return View(); // Returnerer Login viewet
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password) // Enkel innloggingsmetode for eksempelformål
        {
            if (username == "admin" && password == "password123") // Enkel sjekk for eksempelformål
            {
            var claims = new List<Claim> // Legg til nødvendige claims
        {
        new Claim(ClaimTypes.Name, username) // Legg til flere claims etter behov
        };

            var claimsIdentity = new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme); // Bruk av CookieAuthenticationDefaults.AuthenticationScheme

            var authProperties = new AuthenticationProperties // Bruk av AuthenticationProperties
            {
            IsPersistent = true, // Husk meg-funksjonalitet
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30) // Sett utløpstid for cookien
            };

            await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),authProperties); // Bruk av SignInAsync med CookieAuthenticationDefaults.AuthenticationScheme

                return RedirectToAction("Index", "Home"); // Omadresser til ønsket side etter innlogging

            }

            return View(); // Returner til innloggingssiden ved feil

        }
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
