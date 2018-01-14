using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pomelo.AlertPlatform.Incident.Models;

namespace Pomelo.AlertPlatform.Incident.Controllers
{
    public class ApiController : BaseController<IncidentContext>
    {
        [HttpPut]
        [HttpPost]
        [HttpPatch]
        [Route("[controller]/incident")]
        public async Task<IActionResult> Put(string title, string body, int severity, Guid projectId, string secret)
        {
            if (!await DB.Projects.AnyAsync(x => x.Id == projectId && x.Secret == secret))
            {
                return Json(new { code = 403, msg = "项目ID或Secret不正确" });
            }

            var project = await DB.Projects.SingleAsync(x => x.Id == projectId);
            var time = DateTime.UtcNow.AddDays(-1);
            var incident = DB.Incidents.FirstOrDefault(x => x.ProjectId == projectId && x.Title == title && x.Severity == severity && x.CreatedTime >= time);
            if (incident != null)
            {
                incident.CreatedTime = DateTime.UtcNow;
                incident.MitigatedTime = null;
                incident.ResolvedTime = null;
                incident.UserId = null;
                incident.HitCount++;
                incident.Status = IncidentStatus.Active;
            }
            else
            {
                incident = new Models.Incident
                {
                    CreatedTime = DateTime.UtcNow,
                    ProjectId = projectId,
                    Title = title,
                    Summary = body,
                    Severity = severity
                };
                DB.Incidents.Add(incident);
            }

            await DB.SaveChangesAsync();
            return Json(new { code = 200, data = incident.Id });
        }
    }
}
