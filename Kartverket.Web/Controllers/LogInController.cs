using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Kartverket.Web.Models;

namespace Kartverket.Web.Controllers;

public class LogInController : Controller
{
    // blir kalt etter at vi trykker på "LogInTing" lenken i Index viewet
    [HttpGet]
    public ActionResult LogInBruker()
    {
        return View();
    }
}