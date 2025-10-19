using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Kartverket.Web.Models;

namespace Kartverket.Web.Controllers;

public class ObstacleController : Controller
{
    // blir kalt etter at vi trykker på "Register Obstacle" lenken i Index viewet
    [HttpGet]
    public ActionResult DataForm() // GET
    {
        return View(); // Returnerer DataForm viewet
    }


    // blir kalt etter at vi trykker på "Submit Data" knapp i DataForm viewet
    [HttpPost]
    public ActionResult DataForm(ObstacleData obstacledata) // POST
    {
        bool isDraft = false; // Variabel for å sjekke om data er utkast
        if (obstacledata.ObstacleDescription == null) // Sjekker om beskrivelsen er tom
        { 
            isDraft = true; // Setter isDraft til true hvis beskrivelsen er tom
        }

        return View("Overview", obstacledata); // Returnerer Overview viewet med obstacledata
    }
}
