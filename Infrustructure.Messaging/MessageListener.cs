using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;

namespace Infrastructure.Messaging
{
    public abstract class MessageListener<TBus> : IHostedService, IDisposable
        where TBus : MessageBus, new()
    {
        protected readonly IServiceProvider ServiceProvider;

        private readonly IAdvancedBus _easyNetQBus;
        private readonly IOptions<TBus> _messageBus;
        private readonly ILogger<MessageListener<TBus>> _log;
        private readonly AsyncRetryPolicy _retryPolicy;

        private IQueue _queue;
        private IDisposable _subscription;

        protected MessageListener(IServiceProvider serviceProvider,
            IAdvancedBus easyNetQBus,
            IOptions<TBus> messageBus,
            ILogger<MessageListener<TBus>> log)
        {
            ServiceProvider = serviceProvider;
            _easyNetQBus = easyNetQBus;
            _messageBus = messageBus;
            _log = log;
            _retryPolicy = Policy.Handle<Exception>(e => {
                log.LogError(e, "Failed to start message listener.");
                return true;
            }).WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(10));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _retryPolicy.ExecuteAsync(async () => {
                _queue = await _easyNetQBus.DeclareMessageBusAsync(_messageBus.Value);
                _subscription = ConfigureSubscription(_easyNetQBus, _queue);
            });
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Dispose();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (_subscription != null)
            {
                _subscription.Dispose();
                _subscription = null;
            }
        }

        protected abstract IDisposable ConfigureSubscription(IAdvancedBus bus, IQueue queue);

        private async Task<(bool Handled, int RedeliveryCount)> HandleRedelivery(MessageProperties properties, MessageReceivedInfo info, Func<IQueue, Task> requeue)
        {
            var redeliveryCount = 0;

            if (properties.Headers.ContainsKey(MessageHeaders.RedeliveryCount) &&
                properties.Headers[MessageHeaders.RedeliveryCount] is int msgRedeliveryCount)
            {
                redeliveryCount = msgRedeliveryCount;
            }

            if (info.Redelivered)
            {
                var redeliveryLimit = _messageBus.Value.RedeliveryLimit;
                if (properties.Headers.ContainsKey(MessageHeaders.RedeliveryLimit) &&
                    properties.Headers[MessageHeaders.RedeliveryLimit] is int msgRedeliveryLimit)
                {
                    redeliveryLimit = msgRedeliveryLimit;
                }

                if (redeliveryLimit.HasValue && redeliveryCount >= redeliveryLimit)
                {
                    throw new RedeliveryLimitException();
                }

                properties.Headers[MessageHeaders.RedeliveryCount] = redeliveryCount + 1;

                var redeliveryDelay = _messageBus.Value.RedeliveryDelay;
                if (properties.Headers.ContainsKey(MessageHeaders.RedeliveryDelay) &&
                    properties.Headers[MessageHeaders.RedeliveryDelay] is int msgRedeliveryDelay)
                {
                    redeliveryDelay = TimeSpan.FromMilliseconds(msgRedeliveryDelay);
                }

                if (redeliveryDelay.HasValue)
                {
                    properties.Expiration = redeliveryDelay.Value.TotalMilliseconds.ToString("F0");

                    await requeue(await _easyNetQBus.GetDelayQueueAsync(_queue.Name));
                }
                else
                {
                    await requeue(_queue);
                }

                return (true, redeliveryCount);
            }

            return (false, redeliveryCount);
        }

        protected Func<IMessage<T>, MessageReceivedInfo, Task> ConsumerHandler<T>(Func<IMessage<T>, ExtendedMessageReceivedInfo, Task> inner)
        {
            return async (message, info) => {
               var (handled, redeliveryCount) = await HandleRedelivery(message.Properties, info,
                    queue => _easyNetQBus.PublishAsync(Exchange.GetDefault(), queue.Name, true, message)
                );

                if (handled)
                {
                    return;
                }

                _log.LogInformation("Received message. Message queue {queue}, type {type}", info.Queue, typeof(T).Name);

                await inner.Invoke(message, new ExtendedMessageReceivedInfo(info, redeliveryCount));

            };
        }

        protected Func<IMessage<T>, MessageReceivedInfo, Task> ConsumerHandler<T, THandler>()
            where THandler : IMessageHandler<T>
        {
            return ConsumerHandler<T>(async (message, info) => {
                using (var scope = ServiceProvider.CreateScope())
                {
                    var handler = scope.ServiceProvider.GetRequiredService<THandler>();
                    await handler.HandleAsync(message, info);
                }
            });
        }

        protected Func<byte[], MessageProperties, MessageReceivedInfo, Task> JsonConsumerHandler<T>(Func<IMessage<T>, ExtendedMessageReceivedInfo, Task> inner)
        {
            return async (body, properties, info) => {
                var (handled, redeliveryCount) = await HandleRedelivery(properties, info,
                    queue => _easyNetQBus.PublishAsync(Exchange.GetDefault(), queue.Name, true, properties, body)
                );

                if (handled)
                {
                    return;
                }

                var traceId = properties.GetStringHeader(MessageHeaders.TraceId) ?? Guid.NewGuid().ToString();

            };
        }

        protected Func<byte[], MessageProperties, MessageReceivedInfo, Task> JsonConsumerHandler<T, THandler>()
            where THandler : IMessageHandler<T>
        {
            return JsonConsumerHandler<T>(async (message, info) => {
                using (var scope = ServiceProvider.CreateScope())
                {
                    var handler = scope.ServiceProvider.GetRequiredService<THandler>();
                    await handler.HandleAsync(message, info);
                }
            });
        }
    }
}
