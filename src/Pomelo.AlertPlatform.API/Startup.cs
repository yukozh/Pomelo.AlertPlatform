using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.AlertPlatform.API.Models;

namespace Pomelo.AlertPlatform.API
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddConfiguration(out var Config);
            services.AddEntityFrameworkMySql()
                .AddDbContext<AlertContext>(x => {
                    x.UseMySql(Config["Database"]);
                    x.UseMySqlLolita();
                });
            services
                .AddIdentity<User, IdentityRole<Guid>>(x =>
                {
                    x.Password.RequireDigit = false;
                    x.Password.RequiredLength = 0;
                    x.Password.RequireLowercase = false;
                    x.Password.RequireNonAlphanumeric = false;
                    x.Password.RequireUppercase = false;
                    x.User.AllowedUserNameCharacters = null;
                })
              .AddDefaultTokenProviders()
              .AddEntityFrameworkStores<AlertContext>();
            services.AddMvc();
            services.AddSmartUser<User, Guid>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }
}
