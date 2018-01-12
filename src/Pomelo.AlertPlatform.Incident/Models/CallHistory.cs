using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pomelo.AlertPlatform.Incident.Models
{
    public class CallHistory
    {
        public Guid Id{ get; set; }

        [ForeignKey("Incident")]
        public long IncidentId { get; set; }

        public virtual Incident Incident { get; set; }

        public Guid? CallCenterId { get; set; }

        public DateTime CreatedTime { get; set; }
    }
}
