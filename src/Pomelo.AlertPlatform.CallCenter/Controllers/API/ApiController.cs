using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pomelo.AlertPlatform.CallCenter.Models;

namespace Pomelo.AlertPlatform.CallCenter.Controllers
{
    public class ApiController : BaseController<AlertContext>
    {
        [HttpPut("[controller]/message")]
        [HttpPost("[controller]/message")]
        [HttpPatch("[controller]/message")]
        public async Task<IActionResult> PutMessage(string to, string text, int retry, int replay, Guid appId, string secret, MessageType type)
        {
            var app = GetApp(appId, secret);
            if (app == null)
            {
                return Result(404, "Not found");
            }
            else
            {
                var msg = new Message
                {
                    AppId = appId,
                    CreatedTime = DateTime.UtcNow,
                    To = to,
                    Type = type,
                    Text = text,
                    RetryLeft = retry,
                    Replay = replay
                };
                DB.Messages.Add(msg);
                await DB.SaveChangesAsync();
                return Result(200, "Succeeded", msg.Id);
            }
        }

        private async Task<App> GetApp(Guid appId, string secret)
        {
            return await DB.Apps.SingleOrDefaultAsync(x => x.Id == appId && x.Secret == secret);
        }

        private IActionResult Result(int statuscode, string msg, object data = null)
        {
            Response.StatusCode = statuscode;
            return Json(new { code = statuscode, msg, data });
        }
    }
}
