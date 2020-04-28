using System;
using System.Linq;
using EasyNetQ;
using EasyNetQ.Consumer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infrastructure.Messaging
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitMessageBus(this IServiceCollection services)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => assembly.GetName().Name.StartsWith("*", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            services.RegisterEasyNetQ(resolver => {
                var settings = resolver.Resolve<IOptions<RabbitMQConfig>>().Value;
                return new ConnectionConfiguration()
                {
                    Hosts = new[] {
                        new HostConfiguration() {Host = settings.Host, Port = (ushort)settings.Port}
                    },
                    UserName = settings.Username,
                    Password = settings.Password,
                    VirtualHost = settings.VHost,
                    UseBackgroundThreads = true
                };
            }, register => {
                register.Register<IConsumerErrorStrategy, RequeuingConsumerErrorStrategy>();
                register.Register<ITypeNameSerializer>(new TypeAliasTypeNameSerializer(assemblies));
            });

            services.AddSingleton(typeof(IMessageSender<>), typeof(MessageSender<>));

            services.AddHealthChecks()
                .AddCheck<RabbitMQHealthCheck>("rabbitmq");

            return services;
        }
    }
}
