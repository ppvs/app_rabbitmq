using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Internals;
using EasyNetQ.Topology;

namespace Infrastructure.Messaging
{
    internal static class AdvancedBusExtensions
    {
        private static readonly ConcurrentDictionary<string, AsyncLock> DelayQueueLocks = new ConcurrentDictionary<string, AsyncLock>();
        private static readonly Dictionary<string, IQueue> DelayQueues = new Dictionary<string, IQueue>();

        public static string GetDelayQueueName(string queue) => $"{queue}_delay";

        public static IQueue GetDelayQueue(this IAdvancedBus advancedBus, string queue)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            if (DelayQueues.TryGetValue(queue, out var result))
            {
                return result;
            }

            var lockObject = DelayQueueLocks.GetOrAdd(queue, key => new AsyncLock());
            using (lockObject.Acquire())
            {
                // ReSharper disable once InconsistentlySynchronizedField
                if (DelayQueues.TryGetValue(queue, out result))
                {
                    return result;
                }

                result = advancedBus.QueueDeclare(GetDelayQueueName(queue),
                    deadLetterExchange: "",
                    deadLetterRoutingKey: queue
                );

                lock (DelayQueues)
                {
                    DelayQueues.Add(queue, result);
                }

                return result;
            }
        }

        public static async Task<IQueue> GetDelayQueueAsync(this IAdvancedBus advancedBus, string queue)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            if (DelayQueues.TryGetValue(queue, out var result))
            {
                return result;
            }

            var lockObject = DelayQueueLocks.GetOrAdd(queue, key => new AsyncLock());
            using (await lockObject.AcquireAsync())
            {
                // ReSharper disable once InconsistentlySynchronizedField
                if (DelayQueues.TryGetValue(queue, out result))
                {
                    return result;
                }

                result = await advancedBus.QueueDeclareAsync(GetDelayQueueName(queue),
                    deadLetterExchange: "",
                    deadLetterRoutingKey: queue
                );

                lock (DelayQueues)
                {
                    DelayQueues.Add(queue, result);
                }

                return result;
            }
        }

        public static async Task<IQueue> DeclareMessageBusAsync(this IAdvancedBus advancedBus, MessageBus messageBus)
        {
            if (messageBus.DeadLetterMessageBus == null)
            {
                return await advancedBus.QueueDeclareAsync(messageBus.Queue);
            }
            else
            {
                await advancedBus.DeclareMessageBusAsync(messageBus.DeadLetterMessageBus);

                return await advancedBus.QueueDeclareAsync(messageBus.Queue,
                    deadLetterExchange: "",
                    deadLetterRoutingKey: messageBus.DeadLetterMessageBus.Queue
                );
            }
        }
    }
}
