using EasyNetQ;

namespace Infrastructure.Messaging
{
    public class ExtendedMessageReceivedInfo : MessageReceivedInfo
    {
        public int RedeliveryCount { get; set; }

        public ExtendedMessageReceivedInfo()
        {
        }

        public ExtendedMessageReceivedInfo(MessageReceivedInfo info, int redeliveryCount)
            : base(info.ConsumerTag, info.DeliverTag, redeliveryCount > 0, info.Exchange, info.RoutingKey, info.Queue)
        {
            RedeliveryCount = redeliveryCount;
        }

        public override string ToString()
        {
            return $"[ConsumerTag={ConsumerTag}, DeliverTag={DeliverTag}, Redelivered={Redelivered}, Exchange={Exchange}, RoutingKey={RoutingKey}, Queue={Queue}, RedeliveryCount={RedeliveryCount}]";
        }
    }
}
