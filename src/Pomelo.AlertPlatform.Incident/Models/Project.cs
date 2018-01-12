using System;
using System.ComponentModel.DataAnnotations;

namespace Pomelo.AlertPlatform.Incident.Models
{
    public class Project
    {
        public Guid Id { get; set; }

        [MaxLength(128)]
        public string Name { get; set; }

        [MaxLength(128)]
        public string Secret { get; set; }
    }
}
