using System;
using System.Threading.Tasks;

namespace Infrastructure.Messaging
{
    public interface IMessageSender<TBus> where TBus : MessageBus
    {
        Task SendAsync<T>(T message, TimeSpan? redeliveryDelay = null, int? redeliveryLimit = null, bool persistent = true);
    }
}
