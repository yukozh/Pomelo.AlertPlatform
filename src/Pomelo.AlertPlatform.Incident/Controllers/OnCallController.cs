using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Pomelo.Data.Excel;
using Pomelo.AlertPlatform.Incident.Models;
using Pomelo.AlertPlatform.Incident.ViewModels;

namespace Pomelo.AlertPlatform.Incident.Controllers
{
    [Authorize]
    public class OnCallController : BaseController<IncidentContext, User, Guid>
    {
        public async Task<IActionResult> Index(Guid? project, DateTime? begin, DateTime? end, CancellationToken token)
        {
            IQueryable<OnCallSlot> query = DB.OnCallSlots.Include(x => x.User);
            if (project.HasValue)
            {
                query = query.Where(x => x.ProjectId == project);
            }

            if (begin.HasValue)
            {
                query = query.Where(x => x.End >= begin.Value);
            }

            if (end.HasValue)
            {
                query = query.Where(x => x.Begin <= end.Value);
            }

            if (begin.HasValue && !end.HasValue)
            {
                query = query.Where(x => x.Begin <= begin.Value.AddDays(30));
            }

            if (!begin.HasValue && end.HasValue)
            {
                query = query.Where(x => x.End >= end.Value.AddDays(-30));
            }

            if (!begin.HasValue && !end.HasValue)
            {
                var _begin = DateTime.UtcNow.AddDays(-15);
                var _end = DateTime.UtcNow.AddDays(15);
                query = query.Where(x => x.End >= _begin && x.Begin <= _end);
            }

            var result = await query.ToListAsync(token);

            var users = DB.Users
                .Where(u => result.Select(x => x.UserId).Contains(u.Id))
                .Select(x => new { x.Id, x.UserName })
                .ToDictionary(x => x.Id);

            var ret = result.GroupBy(x => new { x.Begin, x.End })
                .Select(x => new OnCallSlotViewModel { Begin = x.Key.Begin, End = x.Key.End, Primary = x.Single(y => y.Role == SlotRole.Primary).User.UserName, Backup = x.Single(y => y.Role == SlotRole.Backup).User.UserName, IncidentManager = x.Single(y => y.Role == SlotRole.IncidentManager).User.UserName })
                .ToList();

            ViewBag.Projects = await DB.Projects.ToListAsync(token);
            return View(ret);
        }

        [HttpGet]
        public async Task<IActionResult> Upload(CancellationToken token)
        {
            ViewBag.Projects = await DB.Projects.ToListAsync(token);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file, Guid projectId)
        {
            var fileStream = file.OpenReadStream();
            var excel = new ExcelStream();
            excel.Load(fileStream);
            var sheet1 = excel.LoadSheetHDR(excel.WorkBook.First().Name);
            var slots = new List<OnCallSlot>(sheet1.Count);
            foreach (var x in sheet1)
            {
                var primary = await User.Manager.FindByNameAsync(x[2]);
                var backup = await User.Manager.FindByNameAsync(x[3]);
                var incidentManager = await User.Manager.FindByNameAsync(x[4]);
                slots.Add(new OnCallSlot
                {
                    Begin = Convert.ToDateTime(x[0]),
                    End = Convert.ToDateTime(x[1]),
                    ProjectId = projectId,
                    Role = SlotRole.Primary,
                    UserId = primary.Id
                });
                slots.Add(new OnCallSlot
                {
                    Begin = Convert.ToDateTime(x[0]),
                    End = Convert.ToDateTime(x[1]),
                    ProjectId = projectId,
                    Role = SlotRole.Backup,
                    UserId = backup.Id
                });
                slots.Add(new OnCallSlot
                {
                    Begin = Convert.ToDateTime(x[0]),
                    End = Convert.ToDateTime(x[1]),
                    ProjectId = projectId,
                    Role = SlotRole.IncidentManager,
                    UserId = incidentManager.Id
                });
            }
            DB.AddRange(slots);
            await DB.SaveChangesAsync();
            return Prompt(x =>
            {
                x.Title = "导入成功";
                x.Details = "On-Call日程表已经成功导入";
                x.HideBack = true;
                x.RedirectUrl = Url.Action("Index", "OnCall", new { project = projectId });
                x.RedirectText = "查看On-Call时间表";
            });
        }
    }
}
