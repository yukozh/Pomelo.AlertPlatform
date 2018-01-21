﻿using System;
using System.ComponentModel.DataAnnotations;
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

        [ForeignKey("User")]
        public Guid UserId { get; set; }

        public virtual User User { get; set; }

        [MaxLength(16)]
        public string Type { get; set; }

        public SlotRole Role { get; set; }

        public bool Ignore { get; set; }
    }
}
