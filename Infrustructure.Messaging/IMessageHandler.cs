using System.Threading.Tasks;
using EasyNetQ;

namespace Infrastructure.Messaging
{
    public interface IMessageHandler<T>
    {
        Task HandleAsync(IMessage<T> message, ExtendedMessageReceivedInfo info);
    }
}
