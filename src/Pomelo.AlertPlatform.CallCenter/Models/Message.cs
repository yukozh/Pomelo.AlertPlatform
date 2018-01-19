using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Pomelo.AlertPlatform.CallCenter.Models
{
    public enum MessageType
    {
        Sms,
        Voice
    }

    public enum MessageStatus
    {
        Pending,
        Succeeded,
        Failed
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

        public int Replay { get; set; } = 1;

        public int RetryLeft { get; set; } = 3;

        public DateTime CreatedTime { get; set; }

        public DateTime? DeliveredTime { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public MessageType Type { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public MessageStatus Status { get; set; }

        public string Error { get; set; }
    }
}
