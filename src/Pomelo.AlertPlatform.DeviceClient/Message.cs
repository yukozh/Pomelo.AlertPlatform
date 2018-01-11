using System;

namespace Pomelo.AlertPlatform.DeviceClient
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
        
        public string To { get; set; }
        
        public string Text { get; set; }

        public int Replay { get; set; } = 1;

        public int RetryLeft { get; set; } = 3;

        public DateTime CreatedTime { get; set; }

        public DateTime? DeliveredTime { get; set; }

        public MessageType Type { get; set; }

        public MessageStatus Status { get; set; }

        public string Error { get; set; }
    }
}
