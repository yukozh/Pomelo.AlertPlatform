using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.AlertPlatform.Incident.Models;

namespace Pomelo.AlertPlatform.Incident
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddConfiguration(out var Config);
            services.AddEntityFrameworkMySql()
                .AddDbContext<IncidentContext>(x => {
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
              .AddEntityFrameworkStores<IncidentContext>();
            services.AddMvc();
            services.AddSmartUser<User, Guid>();
            services.AddCallCenter();
            services.AddTimedJob();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();

            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                app.UseTimedJob();
            }
        }
    }
}
