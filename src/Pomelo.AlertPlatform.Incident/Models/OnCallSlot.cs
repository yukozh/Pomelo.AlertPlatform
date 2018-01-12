using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pomelo.AlertPlatform.Incident.Models
{
    public enum SlotRole
    {
        Primary,
        Backup,
        IncidentManager
    }

    public class OnCallSlot
    {
        [ForeignKey("User")]
        public Guid UserId { get; set; }

        public virtual User User { get; set; }

        public DateTime Begin { get; set; }

        public DateTime End { get; set; }

        [ForeignKey("Project")]
        public Guid ProjectId { get; set; }

        public virtual Project Project { get; set; }
    }
}
