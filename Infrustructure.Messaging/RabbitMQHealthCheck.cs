using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Infrastructure.Messaging
{
    public class RabbitMQHealthCheck : IHealthCheck
    {
        private readonly IAdvancedBus _advancedBus;

        public RabbitMQHealthCheck(IAdvancedBus advancedBus)
        {
            _advancedBus = advancedBus;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
        {
            if (_advancedBus.IsConnected)
            {
                return Task.FromResult(HealthCheckResult.Healthy());
            }
            else
            {
                return Task.FromResult(HealthCheckResult.Degraded("RabbitMQ connection lost."));
            }
        }
    }
}
