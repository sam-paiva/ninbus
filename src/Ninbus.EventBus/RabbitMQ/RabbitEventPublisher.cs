using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace Ninbus.EventBus.RabbitMQ
{
    public class RabbitEventPublisher : IEventPublisher
    {
        private readonly IRabbitConnection _rabbitConnection;
        private readonly ILogger<RabbitEventPublisher> _logger;
        private readonly IExchangeQueueManager _exchangeQueueCreator;
        private readonly NinbusConfiguration _options;

        public RabbitEventPublisher(IRabbitConnection rabbitConnection, IExchangeQueueManager exchangeQueueCreator, NinbusConfiguration options,
            ILogger<RabbitEventPublisher> logger)
        {
            _rabbitConnection = rabbitConnection;
            _exchangeQueueCreator = exchangeQueueCreator;
            _logger = logger;
            _options = options;
        }

        public Task Publish<T>(T @event) where T : IntegrationEvent
        {
            _exchangeQueueCreator.EnsureExchangeIsCreated();
            var eventname = @event.GetType().Name;
            _logger.LogInformation($"Publishing {eventname} with id: {@event.Id}");

            if (!_rabbitConnection.IsConnected)
                _rabbitConnection.TryConnect();

            using var channel = _rabbitConnection.CreateModel();
            var props = channel.CreateBasicProperties();
            props.CorrelationId = @event.Id.ToString();
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event));

            channel.BasicPublish(
                exchange: _options.ExchangeName,
                routingKey: eventname,
                mandatory: true,
                basicProperties: props,
                body: body);
            _logger.LogDebug("Event published");

            return Task.CompletedTask;
        }
    }
}
