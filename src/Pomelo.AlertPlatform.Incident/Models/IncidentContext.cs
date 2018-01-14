using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Pomelo.AlertPlatform.Incident.Models
{
    public class IncidentContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public IncidentContext(DbContextOptions opt) : base(opt)
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
        }

        public DbSet<CallHistory> CallHistories { get; set; }

        public DbSet<Incident> Incidents { get; set; }

        public DbSet<OnCallSlot> OnCallSlots { get; set; }

        public DbSet<Project> Projects { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<CallHistory>(e =>
            {
                e.HasIndex(x => x.CreatedTime);
                e.HasIndex(x => x.CallCenterId);
            });

            builder.Entity<Incident>(e => 
            {
                e.HasIndex(x => x.CreatedTime);
                e.HasIndex(x => x.Severity);
                e.HasIndex(x => x.Status);
                e.HasIndex(x => x.Title).ForMySqlIsFullText();
                e.HasIndex(x => x.MitigatedTime);
                e.HasIndex(x => x.ResolvedTime);
            });

            builder.Entity<OnCallSlot>(e =>
            {
                e.HasKey(x => new { x.Begin, x.End, x.ProjectId, x.Role });
            });
        }
    }
}
