using System;
using EasyNetQ;
using EasyNetQ.Consumer;
using EasyNetQ.Topology;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Messaging
{
    public class RequeuingConsumerErrorStrategy : IConsumerErrorStrategy
    {
        // not injecting IAdvancedBus here because it causes circular dependency:
        // IAdvancedBus -> IConsumerErrorStrategy -> RequeuingConsumerErrorStrategy -> IAdvancedBus
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptions<RequeuingConsumerErrorSettings> _settings;
        private readonly ILogger<RequeuingConsumerErrorStrategy> _log;

        public RequeuingConsumerErrorStrategy(
            IServiceProvider serviceProvider,
            IOptions<RequeuingConsumerErrorSettings> settings,
            ILogger<RequeuingConsumerErrorStrategy> log)
        {
            _serviceProvider = serviceProvider;
            _settings = settings;
            _log = log;
        }

        public void Dispose()
        {
        }

        public virtual AckStrategy HandleConsumerError(ConsumerExecutionContext context, Exception exception)
        {
            if (exception is RedeliveryLimitException)
            {
                return AckStrategies.NackWithoutRequeue;
            }

            _log.LogError(exception, "Consumer error. Message info: {@info}, properties: {@properties}", context.Info, context.Properties);

            if (exception.StackTrace.Contains("EasyNetQ.DefaultMessageSerializationStrategy.DeserializeMessage"))
            {
                var advancedBus = _serviceProvider.GetRequiredService<IAdvancedBus>();
                var delayQueue = advancedBus.GetDelayQueue(context.Info.Queue);
                var properties = context.Properties;

                properties.Expiration = _settings.Value.BrokenMessageRedeliveryDelay.TotalMilliseconds.ToString("F0");

                advancedBus.Publish(Exchange.GetDefault(), delayQueue.Name, true, properties, context.Body);

                return AckStrategies.Ack;
            }

            return AckStrategies.NackWithRequeue;
        }

        public virtual AckStrategy HandleConsumerCancelled(ConsumerExecutionContext context)
        {
            return AckStrategies.NackWithRequeue;
        }
    }
}
