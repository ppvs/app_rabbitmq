using System;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;
using service.api;
using Infrastructure.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace service.Management
{
    public class ProcessingResponseListener : MessageListener<ProcessingResponseBus>
    {

        private ILogger<ProcessingResponseListener> _log;

        public ProcessingResponseListener(
            IServiceProvider serviceProvider,
            IAdvancedBus easyNetQBus,
            IOptions<ProcessingResponseBus> messageBus,
            ILogger<ProcessingResponseListener> log)
            : base(serviceProvider, easyNetQBus, messageBus, log)
        {
            _log = log;
        }

        protected override IDisposable ConfigureSubscription(IAdvancedBus bus, IQueue queue)
        {
            return bus.Consume(queue, ConsumerHandler<ProcessingResponse>(HandleResponse));
        }

        Task HandleResponse(IMessage<ProcessingResponse> response, ExtendedMessageReceivedInfo info)
        {

            if (response.Body.Command is Command importCommand)
            {
                return Task.CompletedTask;
            }
            else
            {
                _log.LogError("Error");
                throw new ArgumentException($"{nameof(ProcessingDeadLetterListener)} unkown command type");
            }
        }
    }
}
