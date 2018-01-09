using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Pomelo.AlertPlatform.API.Models;

namespace Pomelo.AlertPlatform.API.Controllers.Portal
{
    [Authorize]
    public class PortalController : BaseController<AlertContext, User, Guid>
    {
        public static string GetRandomString(int length, bool useNum = true, bool useLow = true, bool useUpp = true, bool useSpe = false, string custom = null)
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

        [Route("/")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("/app")]
        public IActionResult App()
        {
            return PagedView(DB.Apps.Where(x => x.UserId == User.Current.Id));
        }

        [HttpGet]
        [Route("/app/secret")]
        public async Task<IActionResult> AppSecret(Guid id, CancellationToken token)
        {
            var app = await DB.Apps
                .SingleOrDefaultAsync(x => x.Id == id, token);

            if (app == null)
            {
                return Prompt(x =>
                {
                    x.Title = "没有找到应用";
                    x.StatusCode = 404;
                    x.Details = "您指定的应用没有找到";
                });
            }

            if (!User.IsInRole("Root") && User.Current.Id != app.UserId)
            {
                return Prompt(x =>
                {
                    x.Title = "没有权限";
                    x.Details = "您没有权限查看这个App的Secret";
                    x.StatusCode = 401;
                });
            }

            return View(app);
        }

        [HttpGet]
        [Route("/app/new")]
        public IActionResult NewApp()
        {
            return View();
        }

        [HttpPost]
        [Route("/app/new")]
        public async Task<IActionResult> NewApp(string name, CancellationToken token)
        {
            var app = new App
            {
                Name = name,
                Secret = GetRandomString(128),
                UserId = User.Current.Id
            };
            DB.Apps.Add(app);
            await DB.SaveChangesAsync(token);

            return Prompt(x =>
            {
                x.Title = "创建成功";
                x.Details = $"您的App ID为'{app.Id}'，Secret为'{app.Secret}'";
                x.RedirectText = "返回应用列表";
                x.RedirectUrl = Url.Action("App");
                x.HideBack = true;
            });
        }

        [Route("/{type}/history")]
        public async Task<IActionResult> History(MessageType type)
        {
            return PagedView(DB.Messages
                .Where(x => x.App.UserId == User.Current.Id)
                .Where(x => x.Type == type)
                .OrderByDescending(x => x.CreatedTime));
        }
    }
}
