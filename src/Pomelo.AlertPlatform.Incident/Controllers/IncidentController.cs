using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Pomelo.AlertPlatform.Incident.Models;

namespace Pomelo.AlertPlatform.Incident.Controllers
{
    [Authorize]
    public class IncidentController : BaseController<IncidentContext, User, Guid>
    {
        public async Task<IActionResult> Index(IncidentStatus? status, string title, DateTime? begin, DateTime? end, Guid? projectId, CancellationToken token)
        {
            IEnumerable<Models.Incident> query = DB.Incidents
                .Include(x => x.User)
                .Include(x => x.Project);

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
            var incident = await DB.Incidents
                .Include(x => x.User)
                .Include(x => x.Project)
                .Include(x => x.CallHistories)
                .ThenInclude(x => x.User)
                .SingleOrDefaultAsync(x => x.Id == id, token);
            return View(incident);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Acknowledge(long id, CancellationToken token)
        {
            var incident = await DB.Incidents
                .Include(x => x.User)
                .SingleAsync(x => x.Id == id, token);

            if (incident.UserId.HasValue)
            {
                return Prompt(x =>
                {
                    x.Title = "获知故障失败";
                    x.Details = $"该故障已经被{incident.User.UserName}获知";
                    x.StatusCode = 400;
                });
            }

            incident.UserId = User.Current.Id;
            await DB.SaveChangesAsync();
            return Prompt(x =>
            {
                x.Title = "获知故障成功";
                x.Details = $"您已经成功获知该故障";
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resolve(long id, CancellationToken token)
        {
            var incident = await DB.Incidents
                .Include(x => x.User)
                .SingleAsync(x => x.Id == id, token);

            if (!incident.UserId.HasValue)
            {
                return Prompt(x =>
                {
                    x.Title = "解决该故障失败";
                    x.Details = $"该故障尚未被获知";
                    x.StatusCode = 400;
                });
            }

            incident.Status = IncidentStatus.Resolved;
            incident.ResolvedTime = DateTime.UtcNow;
            if (!incident.MitigatedTime.HasValue)
            {
                incident.MitigatedTime = DateTime.UtcNow;
            }

            await DB.SaveChangesAsync();

            return Prompt(x =>
            {
                x.Title = "解决该故障成功";
                x.Details = $"已经将该故障标记为已经解决";
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Mitigate(long id, CancellationToken token)
        {
            var incident = await DB.Incidents
                .Include(x => x.User)
                .SingleAsync(x => x.Id == id, token);

            if (!incident.UserId.HasValue)
            {
                return Prompt(x =>
                {
                    x.Title = "标记已缓解该故障失败";
                    x.Details = $"该故障尚未被获知";
                    x.StatusCode = 400;
                });
            }

            incident.Status = IncidentStatus.Mitigated;
            incident.MitigatedTime = DateTime.UtcNow;

            await DB.SaveChangesAsync();

            return Prompt(x =>
            {
                x.Title = "标记该故障已缓解成功";
                x.Details = $"已经将该故障标记为已缓解";
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactive(long id, CancellationToken token)
        {
            var incident = await DB.Incidents
                .Include(x => x.User)
                .SingleAsync(x => x.Id == id, token);

            if (!incident.UserId.HasValue)
            {
                return Prompt(x =>
                {
                    x.Title = "重新开启故障失败";
                    x.Details = $"该故障已经处于活动状态";
                    x.StatusCode = 400;
                });
            }

            incident.Status = IncidentStatus.Active;
            incident.MitigatedTime = null;
            incident.ResolvedTime = null;
            incident.UserId = null;
            
            DB.CallHistories
                .Where(x => x.IncidentId == incident.Id)
                .SetField(x => x.Ignore).WithValue(true)
                .Update();

            await DB.SaveChangesAsync();

            return Prompt(x =>
            {
                x.Title = "重新开启故障成功";
                x.Details = $"已经将该故障重新开启";
            });
        }
    }
}
