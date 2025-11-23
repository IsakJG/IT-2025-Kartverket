using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Kartverket.Web.Models;
using MySqlConnector;

namespace Kartverket.Web.Controllers;

public class RegistrarController : Controller
{
    public ActionResult RegisterMetode()
    {
        return View("MainPageReg");
    }
}