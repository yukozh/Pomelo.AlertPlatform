﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Pomelo.AlertPlatform.CallCenter.Models;

namespace Pomelo.AlertPlatform.CallCenter.Controllers.Portal
{
    [Authorize]
    public class PortalController : BaseController<AlertContext, User, Guid>
    {
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
            if (type == MessageType.Sms)
            {
                ViewBag.Title = "短消息发送历史";
                ViewBag.Current = "sms-history";
            }
            else
            {
                ViewBag.Title = "电话拨叫历史";
                ViewBag.Current = "phone-history";
            }

            return PagedView(DB.Messages
                .Include(x => x.App)
                .Where(x => x.App.UserId == User.Current.Id)
                .Where(x => x.Type == type)
                .OrderByDescending(x => x.CreatedTime));
        }

        [HttpGet]
        [Route("/{type}/send")]
        public async Task<IActionResult> Send(MessageType type, CancellationToken token)
        {
            if (type == MessageType.Sms)
            {
                ViewBag.Title = "发送短消息";
                ViewBag.Current = "sms-send";
            }
            else
            {
                ViewBag.Title = "发送电话通知";
                ViewBag.Current = "phone-send";
            }

            ViewBag.Apps = await DB.Apps
                .Where(x => x.UserId == User.Current.Id)
                .Select(x => new Tuple<string, string>(x.Id.ToString(), x.Name))
                .ToListAsync(token);

            return View();
        }

        [HttpPost]
        [Route("/{type}/send")]
        public async Task<IActionResult> Send(MessageType type, Guid appId, string to, string text, CancellationToken token)
        {
            var app = await DB.Apps.SingleOrDefaultAsync(x => x.Id == appId, token);

            if (!User.IsInRole("Root") && app.UserId != User.Current.Id)
            {
                return Prompt(x => 
                {
                    x.StatusCode = 401;
                    x.Title = "没有权限";
                    x.Details = "您没有权限使用这个App";
                });
            }

            DB.Messages.Add(new Message
            {
                AppId = appId,
                Replay = 3,
                RetryLeft = 3,
                Status = MessageStatus.Pending,
                Text = text,
                To = to,
                Type = type,
                CreatedTime = DateTime.UtcNow
            });
            await DB.SaveChangesAsync(token);
            return Prompt(x =>
            {
                x.Title = "发送成功";
                x.Details = "您的请求已被接受，我们将立即为您发送通知。";
                x.HideBack = true;
                x.RedirectText = "转到历史记录";
                x.RedirectUrl = Url.Action("History", new { type });
            });
        }

        [HttpGet]
        [Route("/device")]
        [Authorize(Roles = "Root")]
        public IActionResult Device()
        {
            return PagedView(DB.Devices);
        }

        [HttpPost]
        [Route("/device/delete")]
        [Authorize(Roles = "Root")]
        public IActionResult DeleteDevice(Guid id)
        {
            DB.Devices
                .Where(x => x.Id == id)
                .Delete();
            return Prompt(x => 
            {
                x.Title = "删除成功";
                x.Details = "该设备已被删除";
                x.HideBack = true;
                x.RedirectText = "返回设备列表";
                x.RedirectUrl = Url.Action("Device");
            });
        }

        [HttpPost]
        [Authorize(Roles = "Root")]
        [Route("/device/add")]
        public async Task<IActionResult> AddDevice(string phoneNumber, CancellationToken token)
        {
            var device = new Models.Device {
                HeartBeat = DateTime.UtcNow.AddMinutes(-5),
                Secret = GetRandomString(128),
                PhoneNumber = phoneNumber
            };
            DB.Devices.Add(device);
            await DB.SaveChangesAsync(token);
            return Prompt(x =>
            {
                x.Title = "添加成功";
                x.Details = "设备已被添加";
                x.HideBack = true;
                x.RedirectText = "查看Device ID与Secret";
                x.RedirectUrl = Url.Action("DeviceDetail", new { device.Id });
            });
        }


        [HttpGet]
        [Authorize(Roles = "Root")]
        [Route("/device/{id:Guid}/detail")]
        public async Task<IActionResult> DeviceDetail(Guid id, CancellationToken token)
        {
            var device = await DB.Devices.SingleOrDefaultAsync(x => x.Id == id, token);
            if (device == null)
            {
                return Prompt(x => 
                {
                    x.Title = "没有找到设备";
                    x.Details = "没有找到您指定的设备，请返回重试！";
                    x.StatusCode = 404;
                });
            }

            return View(device);
        }

        [HttpGet]
        [Route("/tenant")]
        [Authorize(Roles = "Root")]
        public IActionResult Tenant()
        {
            return PagedView(DB.Users.Include(x => x.Apps));
        }

        [HttpPost]
        [Route("/tenant")]
        [Authorize(Roles = "Root")]
        public async Task<IActionResult> AddTenant(string username, string password, CancellationToken token)
        {
            if (await DB.Users.AnyAsync(x => x.UserName == username, token))
            {
                return Prompt(x => 
                {
                    x.Title = "添加失败";
                    x.Details = "用户名已经存在";
                    x.StatusCode = 400;
                });
            }

            var user = new User { UserName = username };
            await User.Manager.CreateAsync(user, password);
            return Prompt(x =>
            {
                x.Title = "添加成功";
                x.Details = "租户已被创建";
                x.HideBack = true;
                x.RedirectText = "转至租户列表";
                x.RedirectUrl = Url.Action("Tenant");
            });
        }
    }
}
