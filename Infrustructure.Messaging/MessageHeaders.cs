using System.Text;
using EasyNetQ;

namespace Infrastructure.Messaging
{
    public static class MessageHeaders
    {
        public const string TraceId = "x-trace-id";
        public const string RedeliveryCount = "x-redelivery-count";
        public const string RedeliveryLimit = "x-redelivery-limit";
        public const string RedeliveryDelay = "x-redelivery-delay";

        public static string GetStringHeader(this MessageProperties properties, string header)
        {
            if (properties.Headers.TryGetValue(header, out var obj) && obj is byte[] data)
            {
                return Encoding.UTF8.GetString(data);
            }

            return null;
        }
    }
}
