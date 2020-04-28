using Infrastructure.Messaging;

namespace service.api
{
    public class ProcessingCommandBus : MessageBus
    {
        public override string Queue => "processing_commands";
        public override int? RedeliveryLimit => 2;
        public override MessageBus DeadLetterMessageBus => ProcessingDeadLetterBus.Instance;
    }

    public class ProcessingDeadLetterBus : MessageBus
    {
        internal static readonly MessageBus Instance = new ProcessingDeadLetterBus();

        public override string Queue => "processing_commands_dlq";
    }
}
