using System;

namespace Infrastructure.Messaging
{
    public class RequeuingConsumerErrorSettings
    {
        public TimeSpan BrokenMessageRedeliveryDelay { get; set; } = TimeSpan.FromMinutes(1);
    }
}
