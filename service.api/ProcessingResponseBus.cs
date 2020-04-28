using Infrastructure.Messaging;

namespace service.api
{
    public class ProcessingResponseBus : MessageBus
    {
        public override string Queue => "processing_response";
    }
}
