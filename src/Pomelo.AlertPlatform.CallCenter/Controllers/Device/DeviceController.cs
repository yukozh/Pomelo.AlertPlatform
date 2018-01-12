using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pomelo.AlertPlatform.CallCenter.Models;

namespace Pomelo.AlertPlatform.CallCenter.Controllers.Device
{
    public class DeviceController : BaseController<AlertContext>
    {
        [HttpPost]
        public async Task<IActionResult> GetTask(Guid device, string secret, CancellationToken token)
        {
            if (!await DB.Devices.AnyAsync(x => x.Id == device && x.Secret == secret, token))
            {
                return Json(new
                {
                    code = 403,
                    msg = "Invalid credential"
                });
            }

            DB.Devices
                .Where(x => x.Id == device)
                .SetField(x => x.HeartBeat).WithValue(DateTime.UtcNow)
                .Update();

            if (await DB.Messages.AnyAsync(x => x.DeviceId == device && x.Status == MessageStatus.Pending))
            {
                var msg = await DB.Messages.FirstAsync(x => x.DeviceId == device && x.Status == MessageStatus.Pending, token);
                return Json(new
                {
                    code = 200,
                    data = msg
                });
            }

            var count = DB.Messages
                .Where(x => x.Status == MessageStatus.Pending)
                .Where(x => x.DeviceId == null)
                .OrderBy(x => x.CreatedTime)
                .SetField(x => x.DeviceId).WithValue(device)
                .Update();

            if (count == 0)
            {
                Response.StatusCode = 404;
                return Json(new
                {
                    code = 404,
                    msg = "没有任务可用"
                });
            }

            var task = await DB.Messages.FirstAsync(x => x.DeviceId == device && x.Status == MessageStatus.Pending, token);
            return Json(new
            {
                code = 200,
                data = task
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTask(Guid message, string error, MessageStatus? status, int? retryLeft, Guid device, string secret, CancellationToken token)
        {
            if (!await DB.Devices.AnyAsync(x => x.Id == device && x.Secret == secret, token))
            {
                return Json(new
                {
                    code = 403,
                    msg = "Invalid credential"
                });
            }

            var msg = await DB.Messages.SingleAsync(x => x.Id == message, token);
            if (msg.DeviceId != device)
            {
                return Json(new
                {
                    code = 401,
                    msg = "No permission"
                });
            }

            if (!string.IsNullOrEmpty(error))
                msg.Error = error;

            if (status.HasValue)
            {
                msg.Status = status.Value;
                msg.DeliveredTime = DateTime.UtcNow;
            }

            if (retryLeft.HasValue)
                msg.RetryLeft = retryLeft.Value;

            await DB.SaveChangesAsync();

            return Json(new {
                code = 200
            });
        }
    }
}
