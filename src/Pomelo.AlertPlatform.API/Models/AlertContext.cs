using Microsoft.EntityFrameworkCore;

namespace Pomelo.AlertPlatform.API.Models
{
    public class AlertContext : DbContext
    {
        public DbSet<App> Apps { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Message> Messages { get; set; }

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
            });
        }
    }
}
