using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Kartverket.Web.Controllers
{
    /// <summary>
    /// Dashboard-kontroller for brukere med rollen "Pilot".
    /// Fungerer som inngangsport for funksjonalitet knyttet til registrering av hindre.
    /// </summary>
    [Authorize(Roles = "Pilot")]
    public class PilotController : Controller
    {
        private readonly ILogger<PilotController> _logger;

        public PilotController(ILogger<PilotController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Viser hovedsiden (Dashboard) for piloter.
        /// </summary>
        [HttpGet]
        public IActionResult MainPage()
        {
            // Det kan være nyttig å logge at dashboardet ble åpnet for analyseformål
            _logger.LogInformation("Pilot åpnet dashboard.");
            return View();
        }
    }
}