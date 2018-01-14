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
    public class ProjectController : BaseController<IncidentContext, User, Guid>
    {
        public async Task<IActionResult> Index(string name, CancellationToken token)
        {
            IEnumerable<Project> query = DB.Projects;

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(x => x.Name.Contains(name));
            }

            ViewBag.Projects = await DB.Projects.ToListAsync(token);
            return PagedView(query);
        }

        public async Task<IActionResult> Detail(Guid id, CancellationToken token)
        {
            var project = await DB.Projects.SingleOrDefaultAsync(x => x.Id == id, token);
            ViewBag.IncidentCount = await DB.Incidents.CountAsync(x => x.ProjectId == id, token);
            ViewBag.ActiveIncidentCount = await DB.Incidents.CountAsync(x => x.ProjectId == id && x.Status == IncidentStatus.Active, token);
            return View(project);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(string name, CancellationToken token)
        {
            var project = new Project
            {
                Name = name,
                Secret = GetRandomString(128)
            };
            DB.Projects.Add(project);
            await DB.SaveChangesAsync(token);

            return Prompt(x =>
            {
                x.Title = "创建成功";
                x.Details = "项目已经成功创建";
                x.RedirectText = "查看Secret";
                x.RedirectUrl = Url.Action("Detail", new { id = project.Id });
                x.HideBack = true;
            });
        }

        private static string GetRandomString(int length, bool useNum = true, bool useLow = true, bool useUpp = true, bool useSpe = false, string custom = null)
        {
            byte[] b = new byte[4];
            new System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(b);
            Random r = new Random(BitConverter.ToInt32(b, 0));
            string s = null, str = custom ?? "";
            if (useNum == true) { str += "0123456789"; }
            if (useLow == true) { str += "abcdefghijklmnopqrstuvwxyz"; }
            if (useUpp == true) { str += "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; }
            if (useSpe == true) { str += "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~"; }
            for (int i = 0; i < length; i++)
            {
                s += str.Substring(r.Next(0, str.Length - 1), 1);
            }
            return s;
        }
    }
}
