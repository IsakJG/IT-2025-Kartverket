using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Kartverket.Web.Controllers
{
    [Authorize(Roles = "Registrar")]
    public class RegistrarController : Controller//Håndterer autorisering av Registrar
    {
        public IActionResult MainPage()
        {
            return View();
        }
    }
}