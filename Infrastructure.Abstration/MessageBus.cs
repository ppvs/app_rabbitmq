using System;

namespace Infrastructure.Messaging
{
    public abstract class MessageBus
    {
        private static readonly TimeSpan DefaultRedeliveryDelay = TimeSpan.FromSeconds(1);

        public abstract string Queue { get; }
        public virtual TimeSpan? RedeliveryDelay => DefaultRedeliveryDelay;
        public virtual int? RedeliveryLimit => null;
        public virtual MessageBus DeadLetterMessageBus => null;
    }
}
