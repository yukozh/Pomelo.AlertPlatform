﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pomelo.AlertPlatform.API.Models;

namespace Pomelo.AlertPlatform.API.Controllers
{
    public class ApiController : BaseController<AlertContext>
    {
        [HttpPut("message")]
        [HttpPost("message")]
        [HttpPatch("message")]
        public async Task<IActionResult> PutMessage(string to, string text, string phoneNumber, int retry, Guid appId, string secret, MessageType type)
        {
            var app = GetApp(appId, secret);
            if (app == null)
            {
                return Result(404, "Not found");
            }
            else
            {
                DB.Messages.Add(new Message
                {
                    AppId = appId,
                    CreatedTime = DateTime.UtcNow,
                    To = to,
                    Type = type,
                    Text = text,
                    RetryLeft = retry
                });
                await DB.SaveChangesAsync();
                return Result(200, "Succeeded");
            }
        }

        private async Task<App> GetApp(Guid appId, string secret)
        {
            return await DB.Apps.SingleOrDefaultAsync(x => x.Id == appId && x.Secret == secret);
        }

        private IActionResult Result(int statuscode, string msg)
        {
            Response.StatusCode = statuscode;
            return Json(new { code = statuscode, msg = msg });
        }
    }
}
