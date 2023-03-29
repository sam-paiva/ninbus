using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Ninbus.EventBus.RabbitMQ
{
    public class RabbitEventSubscriber : IEventSubscriber
    {
        private readonly ISubscriptionManager _subsManager;
        private readonly IRabbitConnection _rabbitConnection;
        private readonly IRabbitConsumerInitializer _rabbitConsumerInitializer;
        private readonly ILogger<RabbitEventSubscriber> _logger;
        private readonly IExchangeQueueManager _exchangeQueueCreator;
        private readonly NinbusConfiguration _options;

        public RabbitEventSubscriber(ISubscriptionManager subsManager, IRabbitConnection rabbitConnection, IRabbitConsumerInitializer rabbitConsumerInitializer,
            ILogger<RabbitEventSubscriber> logger, IExchangeQueueManager exchangeQueueCreator, NinbusConfiguration options)
        {
            _subsManager = subsManager;
            _rabbitConnection = rabbitConnection;
            _rabbitConsumerInitializer = rabbitConsumerInitializer;
            _logger = logger;
            _exchangeQueueCreator = exchangeQueueCreator;
            _options = options;
        }

        public Task StartListeningAsync() => _rabbitConsumerInitializer.InitializeConsumersChannelAsync();

        public Subscription<T> Subscribe<T>() where T : IntegrationEvent
        {
            var subscription = _subsManager.AddSubscription<T>();
            SubscribeToRabbit(subscription);
            return subscription;
        }

        private void SubscribeToRabbit<T>(Subscription<T> subscription) where T : IntegrationEvent
        {
            _exchangeQueueCreator.EnsureExchangeIsCreated();
            _exchangeQueueCreator.EnsureQueueIsCreated();

            _logger.LogInformation($"Binding Queue to exchange with Event {subscription.EventName}");
            using var channel = _rabbitConnection.CreateModel();
            channel.QueueBind(queue: _options.QueueName,
                exchange: _options.ExchangeName,
                routingKey: subscription.EventName, arguments: null);

            channel.QueueBind(queue: _options.DeadLetterName, exchange: _options.DeadLetterName, routingKey: subscription.EventName, arguments: null);
        }
    }
}
