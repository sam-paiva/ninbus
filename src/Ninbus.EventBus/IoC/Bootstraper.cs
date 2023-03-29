using Microsoft.Extensions.DependencyInjection;
using Ninbus.EventBus.RabbitMQ;
using RabbitMQ.Client;
using System.Reflection;

namespace Ninbus.EventBus.IoC
{
    public static class Bootstraper
    {
        public static void UseNinbus(this IServiceCollection services, NinbusConfiguration options, Assembly assembly)
        {
            services.AddMediatR(c => c.RegisterServicesFromAssembly(assembly));

            services.AddSingleton<ISubscriptionManager, SubscriptionManager>();
            services.AddSingleton<IRabbitConnection, RabbitConnection>();
            services.AddScoped<IEventPublisher, RabbitEventPublisher>();
            services.AddSingleton<IRabbitConsumerInitializer, RabbitConsumerInitializer>();
            services.AddSingleton<IRabbitConsumerHandler, RabbitConsumerHandler>();
            services.AddScoped<IEventSubscriber, RabbitEventSubscriber>();
            services.AddSingleton<IExchangeQueueManager, ExchangeQueueManager>();
            services.AddSingleton<IFailureEventService, RabbitFailureEventService>();
            services.AddSingleton<NinbusConfiguration>(options);

            services.AddSingleton<IConnectionFactory>(c =>
            {
                return new ConnectionFactory()
                {
                    VirtualHost = options.VirtualHost,
                    HostName = options.HostName,
                    UserName = options.Username,
                    Password = options.Password,
                    Port = options.Port,
                    ContinuationTimeout = TimeSpan.FromSeconds(30),
                    RequestedConnectionTimeout = TimeSpan.FromSeconds(30),
                    HandshakeContinuationTimeout = TimeSpan.FromSeconds(30),
                    SocketReadTimeout = TimeSpan.FromSeconds(30),
                    SocketWriteTimeout = TimeSpan.FromSeconds(30)
                };
            });
        }
    }
}
