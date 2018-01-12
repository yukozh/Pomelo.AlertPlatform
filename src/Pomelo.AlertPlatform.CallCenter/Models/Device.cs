using System;
using System.ComponentModel.DataAnnotations;

namespace Pomelo.AlertPlatform.CallCenter.Models
{
    public class Device
    {
        public Guid Id { get; set; }

        [MaxLength(128)]
        public string Secret { get; set; }

        [MaxLength(32)]
        public string PhoneNumber { get; set; }

        public DateTime HeartBeat { get; set; } = DateTime.UtcNow;
    }
}
