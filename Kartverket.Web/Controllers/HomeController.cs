using Kartverket.Web.Data;
using Kartverket.Web.Models;
using Kartverket.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Kartverket.Web.Controllers
{
    /// <summary>
    /// Hovedkontroller for generelle sidevisninger, feilhåndtering og brukerinnstillinger.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly KartverketDbContext _db;
        private readonly ILogger<HomeController> _logger;

        // Konstanter for session-nøkler
        private const string SessionKeyUserId = "UserId";
        private const string SessionKeyDarkMode = "DarkMode";

        public HomeController(KartverketDbContext db, ILogger<HomeController> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Landingssiden for applikasjonen.
        /// </summary>
        public IActionResult Index() 
        {
            return View(); 
        }

        /// <summary>
        /// Hovedsiden etter innlogging (Dashboard).
        /// </summary>
        public IActionResult MainPage() 
        {
            return View(); 
        }

        /// <summary>
        /// Personvernerklæring.
        /// </summary>
        public IActionResult Privacy() 
        {
            return View(); 
        }

        /// <summary>
        /// Standard feilside.
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)] 
        public IActionResult Error() 
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }); 
        }

        /// <summary>
        /// AJAX-endepunkt for å sette Dark Mode-preferanse i sesjonen.
        /// </summary>
        /// <param name="request">DTO som inneholder status for dark mode.</param>
        [HttpPost]
        public IActionResult SetDarkMode([FromBody] DarkModeRequest request)
        {
            if (request == null)
            {
                return BadRequest();
            }

            HttpContext.Session.SetString(SessionKeyDarkMode, request.IsDarkMode.ToString().ToLower());
            return Ok();
        }

        /// <summary>
        /// Viser skjema for endring av passord.
        /// </summary>
        [HttpGet]
        public IActionResult PasswordChange() 
        {
            int? userId = HttpContext.Session.GetInt32(SessionKeyUserId);
            
            if (userId == null)
            {
                // Hvis sesjonen er utløpt, send brukeren til innlogging
                return RedirectToAction("Login", "Auth");
            }

            var model = new ChangePasswordViewModel { Id = userId.Value };
            return View(model);
        }

        /// <summary>
        /// Behandler endring av passord.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PasswordChange(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == model.Id);
                
                if (user == null)
                {
                    _logger.LogWarning("Forsøk på å endre passord for ukjent brukerID: {Id}", model.Id);
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(MainPage));
                }

                // Hasher det nye passordet før lagring
                user.PasswordHash = PasswordHasher.HashPassword(model.Password);
                
                _db.Users.Update(user);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Passord endret for bruker: {Username}", user.Username);
                TempData["SuccessMessage"] = "Password updated successfully.";
                
                return RedirectToAction(nameof(MainPage));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved endring av passord for brukerID {Id}", model.Id);
                ModelState.AddModelError(string.Empty, "Could not change password due to a technical error.");
                return View(model);
            }
        }
    }

    /// <summary>
    /// DTO (Data Transfer Object) for Dark Mode forespørsler.
    /// </summary>
    public class DarkModeRequest
    {
        public bool IsDarkMode { get; set; }
    }
}