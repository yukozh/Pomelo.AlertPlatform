using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Pomelo.AlertPlatform.API.Models;

namespace Pomelo.AlertPlatform.API.Controllers
{
    public class AccountController : BaseController<AlertContext, User, Guid>
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            await SignInManager.SignInAsync(await User.Manager.FindByNameAsync(username), true);
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await SignInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
