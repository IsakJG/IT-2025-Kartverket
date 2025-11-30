using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Kartverket.Web.Models;
using MySqlConnector;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization;

namespace Kartverket.Web.Controllers;

[Authorize(Roles = "Registar")]
public class RegistrarController : Controller
{
    public ActionResult RegisterMetode()
    {
        return View("MainPageReg");
    }
}