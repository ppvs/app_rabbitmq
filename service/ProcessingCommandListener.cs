using System;
using EasyNetQ;
using EasyNetQ.Topology;
using service.api;
using Infrastructure.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace service
{
    public class ProcessingCommandListener : MessageListener<ProcessingCommandBus>
    {
        public ProcessingCommandListener(
            IServiceProvider serviceProvider,
            IAdvancedBus easyNetQBus,
            IOptions<ProcessingCommandBus> messageBus,
            ILogger<ProcessingCommandListener> log)
        : base(serviceProvider, easyNetQBus, messageBus, log)
        {
        }

        protected override IDisposable ConfigureSubscription(IAdvancedBus bus, IQueue queue)
        {
            return bus.Consume(
                queue,
                ConsumerHandler<IProcessingCommand, ProcessingCommandHandler>(),
                conf => conf.WithPrefetchCount(1)
            );
        }
    }
}
