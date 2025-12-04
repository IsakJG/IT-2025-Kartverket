using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Kartverket.Web.Data;
using Kartverket.Web.Models;
using Kartverket.Web.Services;

namespace Kartverket.Web.Controllers
{
    /// <summary>
    /// Kontroller for brukere med rollen "Registar" (Matrikkelfører).
    /// </summary>
    // MERK: Rollenavnet er "Registar" (Legacy naming convention), ikke "Registrar".
    // Dette må matche nøyaktig med databasens RoleName-kolonne.
    [Authorize(Roles = "Registar")] 
    public class RegistrarController : Controller
    {
        private readonly KartverketDbContext _db;
        private readonly ILogger<RegistrarController> _logger;

        private const string SessionKeyUserId = "UserId";

        public RegistrarController(KartverketDbContext db, ILogger<RegistrarController> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Hovedsiden for Registar.
        /// </summary>
        [HttpGet]
        public IActionResult RegisterMetode()
        {
            return View("MainPageReg");
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
                // Sender bruker tilbake til hovedsiden for rollen hvis sesjon mangler
                return RedirectToAction(nameof(RegisterMetode));
            }

            var model = new ChangePasswordViewModel { Id = userId.Value };
            return View(model);
        }

        /// <summary>
        /// Behandler passordendring.
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
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(RegisterMetode));
                }

                // Hasher passordet før lagring
                user.PasswordHash = PasswordHasher.HashPassword(model.Password);
                
                _db.Users.Update(user);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Registar (ID: {Id}) endret passordet sitt.", model.Id);
                TempData["SuccessMessage"] = "Password updated successfully.";
                
                return RedirectToAction(nameof(RegisterMetode));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feil ved passordbytte for bruker {Id}", model.Id);
                ModelState.AddModelError(string.Empty, "An error occurred.");
                return View(model);
            }
        }
    }
}