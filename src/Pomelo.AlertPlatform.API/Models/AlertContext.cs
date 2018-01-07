using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Pomelo.AlertPlatform.API.Models
{
    public class AlertContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public AlertContext(DbContextOptions opt) : base(opt)
        {
            this.Database.EnsureCreated();
            if (!Roles.Any(x => x.NormalizedName == "ROOT"))
            {
                Roles.Add(new IdentityRole<Guid>
                {
                    Name = "Root",
                    NormalizedName = "ROOT"
                });
                SaveChanges();
            }

            var time = DateTime.UtcNow.AddYears(-1);
            this.Messages
                .Where(x => x.CreatedTime < time)
                .Delete();
        }

        public DbSet<App> Apps { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Configuration> Configurations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<App>(e =>
            {
                e.HasIndex(x => x.Name).ForMySqlIsFullText();
                e.HasIndex(x => x.Secret);
            });

            builder.Entity<Device>(e =>
            {
                e.HasIndex(x => x.PhoneNumber).ForMySqlIsFullText();
                e.HasIndex(x => x.Secret);
            });

            builder.Entity<Message>(e => 
            {
                e.HasIndex(x => x.Type);
                e.HasIndex(x => x.CreatedTime);
                e.HasIndex(x => x.DeliveredTime);
                e.HasIndex(x => x.To).ForMySqlIsFullText();
                e.HasIndex(x => x.Status);
            });
        }
    }
}
