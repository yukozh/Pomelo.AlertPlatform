using System;
using System.Linq;
using System.Threading;
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
            if (!DB.Apps.Any(x => x.Id == appId && x.Secret == secret))
            {
                return Result(404, "Not found");
            }
            else
            {
                var msg = new Message
                {
                    Id = Guid.NewGuid(),
                    AppId = appId,
                    CreatedTime = DateTime.UtcNow,
                    To = to,
                    Type = type,
                    Text = text,
                    RetryLeft = retry,
                    Replay = replay
                };

                DB.Database.ExecuteSqlCommand("INSERT INTO `messages` (`Id`, `AppId`, `CreatedTime`, `To`, `Type`, `Text`, `RetryLeft`, `Replay`) VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})", 
                    msg.Id,
                    msg.AppId, 
                    msg.CreatedTime, 
                    msg.To,
                    msg.Type,
                    msg.Text,
                    msg.RetryLeft,
                    msg.Replay);

                return Result(200, "Succeeded", msg.Id);
            }
        }

        [HttpGet("[controller]/message")]
        public async Task<IActionResult> GetMessage(Guid id, Guid appId, string secret, CancellationToken token)
        {
            if (!DB.Apps.Any(x => x.Id == appId && x.Secret == secret))
            {
                return Result(404, "Not found");
            }
            else
            {
                var msg = await DB.Messages.SingleOrDefaultAsync(x => x.Id == id, token);
                if (msg == null)
                {
                    return Result(404, "Not Found");
                }
                else
                {
                    return Result(200, "Succeeded", msg);
                }
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
