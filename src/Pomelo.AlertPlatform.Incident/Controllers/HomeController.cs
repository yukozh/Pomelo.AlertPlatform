using System;
using Microsoft.AspNetCore.Mvc;
using Pomelo.AlertPlatform.Incident.Models;

namespace Pomelo.AlertPlatform.Incident.Controllers
{
    public class HomeController : BaseController<IncidentContext, User, Guid>
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
