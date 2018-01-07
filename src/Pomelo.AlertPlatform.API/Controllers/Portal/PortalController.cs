using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Pomelo.AlertPlatform.API.Models;

namespace Pomelo.AlertPlatform.API.Controllers.Portal
{
    [Authorize]
    public class PortalController : BaseController<AlertContext, User, Guid>
    {
        [Route("/")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("/{type:MessageType}/history")]
        public async Task<IActionResult> History(MessageType type)
        {
            return PagedView(DB.Messages
                .Where(x => x.App.UserId == User.Current.Id)
                .Where(x => x.Type == type)
                .OrderByDescending(x => x.CreatedTime));
        }
    }
}
