using Kartverket.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Kartverket.Web.Controllers
{
    public class MapController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ShowLocation(MapViewModel model)
        {
            //Gjør at man kan sende koordinater(long, lat) videre til et View
            return View("LocationResult", model);
        }
    }
}