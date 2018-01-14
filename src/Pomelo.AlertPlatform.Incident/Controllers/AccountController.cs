using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Pomelo.AlertPlatform.Incident.Models;

namespace Pomelo.AlertPlatform.Incident.Controllers
{
    public class AccountController : BaseController<IncidentContext, User, Guid>
    {
        public IActionResult Index()
        {
            return PagedView(DB.Users);
        }

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
