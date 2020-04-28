using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Internals;
using EasyNetQ.Topology;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Messaging
{
    public class MessageSender<TBus> : IMessageSender<TBus>
        where TBus : MessageBus, new()
    {
        private readonly IAdvancedBus _easyNetQBus;
        private readonly IOptions<TBus> _messageBus;
        private readonly ILogger<MessageSender<TBus>> _log;

        public MessageSender(IAdvancedBus easyNetQBus, IOptions<TBus> messageBus, ILogger<MessageSender<TBus>> log)
        {
            _easyNetQBus = easyNetQBus;
            _messageBus = messageBus;
            _log = log;
        }

        private readonly AsyncLock _initializeLock = new AsyncLock();
        private bool _initialized;

        private async Task InitAsync()
        {
            if (_initialized)
            {
                return;
            }

            using (await _initializeLock.AcquireAsync())
            {
                if (_initialized)
                {
                    return;
                }

                await _easyNetQBus.DeclareMessageBusAsync(_messageBus.Value);

                _initialized = true;
            }
        }

        public async Task SendAsync<T>(T message, TimeSpan? redeliveryDelay = null, int? redeliveryLimit = null, bool persistent = true)
        {
            await InitAsync();

            var wrapper = new Message<T>(message);
            
            if (redeliveryDelay.HasValue)
            {
                wrapper.Properties.Headers[MessageHeaders.RedeliveryDelay] = (int)redeliveryDelay.Value.TotalMilliseconds;
            }

            if (redeliveryLimit.HasValue)
            {
                wrapper.Properties.Headers[MessageHeaders.RedeliveryLimit] = redeliveryLimit.Value;
            }

            if (persistent)
            {
                wrapper.Properties.DeliveryMode = 2;
            }

            await _easyNetQBus.PublishAsync(Exchange.GetDefault(), _messageBus.Value.Queue, true, wrapper);
        }

    }
}
