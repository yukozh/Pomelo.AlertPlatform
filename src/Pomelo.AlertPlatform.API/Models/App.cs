using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pomelo.AlertPlatform.API.Models
{
    public class App
    {
        public Guid Id { get; set; }

        [MaxLength(128)]
        public string Secret { get; set; }

        [MaxLength(128)]
        public string Name { get; set; }

        [ForeignKey("User")]
        public Guid UserId { get; set; }

        public virtual User User { get; set; }
    }
}
