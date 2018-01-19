using System;
using Microsoft.AspNetCore.Mvc;
using Pomelo.AlertPlatform.Incident.Models;
using Microsoft.AspNetCore.Authorization;

namespace Pomelo.AlertPlatform.Incident.Controllers
{
    [Authorize]
    public class HomeController : BaseController<IncidentContext, User, Guid>
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
