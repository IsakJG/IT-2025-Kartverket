using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Kartverket.Web.Controllers
{
    [Authorize(Roles = "Pilot")]
    public class PilotController : Controller//Håndterer autorisering av Pilot rolle.
    {
        public IActionResult MainPage()
        {
            return View();
        }
    }
}