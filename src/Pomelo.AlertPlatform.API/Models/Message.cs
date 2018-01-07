using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pomelo.AlertPlatform.API.Models
{
    public enum MessageType
    {
        Sms,
        Voice
    }

    public class Message
    {
        public Guid Id { get; set; }

        [ForeignKey("App")]
        public Guid AppId { get; set; }

        public virtual App App { get; set; }

        [ForeignKey("Device")]
        public Guid? DeviceId { get; set; }

        public virtual Device Device { get; set; }

        [MaxLength(32)]
        public string To { get; set; }

        [MaxLength(512)]
        public string Text { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime? DeliveredTime { get; set; }

        public MessageType Type { get; set; }
    }
}
