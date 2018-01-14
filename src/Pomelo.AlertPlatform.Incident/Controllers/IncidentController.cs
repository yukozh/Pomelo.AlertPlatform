using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pomelo.AlertPlatform.Incident.Models;

namespace Pomelo.AlertPlatform.Incident.Controllers
{
    public class IncidentController : BaseController<IncidentContext, User, Guid>
    {
        public async Task<IActionResult> Index(IncidentStatus? status, string title, DateTime? begin, DateTime? end, Guid? projectId, CancellationToken token)
        {
            IEnumerable<Models.Incident> query = DB.Incidents;

            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }

            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(x => x.Title.Contains(title));
            }

            if (begin.HasValue)
            {
                query = query.Where(x => x.CreatedTime >= begin.Value);
            }

            if (end.HasValue)
            {
                query = query.Where(x => x.CreatedTime <= end.Value);
            }

            if (projectId.HasValue)
            {
                query = query.Where(x => x.ProjectId == projectId.Value);
            }

            ViewBag.Projects = await DB.Projects.ToListAsync(token);
            return PagedView(query.OrderByDescending(x => x.CreatedTime));
        }

        public async Task<IActionResult> Detail(long id, CancellationToken token)
        {
            var incident = await DB.Incidents.SingleOrDefaultAsync(x => x.Id == id, token);
            return View(incident);
        }
    }
}
