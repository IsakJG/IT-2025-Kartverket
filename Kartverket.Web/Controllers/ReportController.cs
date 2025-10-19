using Microsoft.AspNetCore.Mvc;

namespace Kartverket.Web.Controllers
{
    public class ReportController : Controller
    {
        public IActionResult Archive()
        {
            return View();
        }
    }
}
