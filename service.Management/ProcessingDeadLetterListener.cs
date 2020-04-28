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
    public class ProcessingDeadLetterListener : MessageListener<ProcessingDeadLetterBus>
    {

        public ProcessingDeadLetterListener(
            IServiceProvider serviceProvider,
            IAdvancedBus easyNetQBus,
            IOptions<ProcessingDeadLetterBus> messageBus,
            ILogger<ProcessingDeadLetterListener> log)
            : base(serviceProvider, easyNetQBus, messageBus, log)
        {

        }

        protected override IDisposable ConfigureSubscription(IAdvancedBus bus, IQueue queue)
        {
            return bus.Consume(queue, ConsumerHandler<IProcessingCommand>(HandleDeadLetter));
        }

        Task HandleDeadLetter(IMessage<IProcessingCommand> command, ExtendedMessageReceivedInfo info)
        {

            if (command.Body is Command importCommand)
            {
                return Task.CompletedTask;
            }
            else
            {

                throw new ArgumentException($"{nameof(ProcessingDeadLetterListener)} unkown command type");
            }
        }
    }
}
