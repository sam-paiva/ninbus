using RabbitMQ.Client;

namespace Ninbus.EventBus.RabbitMQ
{
    public class ExchangeQueueManager : IExchangeQueueManager
    {
        private readonly IRabbitConnection _rabbitConnection;
        private readonly NinbusConfiguration _rabbitOptions;
        private bool _exchangeCreated;
        private bool _queueCreated;

        public ExchangeQueueManager(IRabbitConnection rabbitConnection, NinbusConfiguration options)
        {
            _rabbitConnection = rabbitConnection;
            _rabbitOptions = options;
        }

        public void EnsureExchangeIsCreated()
        {
            if (!_exchangeCreated)
            {
                EnsureRabbitIsConnected();
                using (var channel = _rabbitConnection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: _rabbitOptions.ExchangeName, type: ExchangeType.Topic);
                    _exchangeCreated = true;
                }
            }
        }

        public void EnsureQueueIsCreated()
        {
            if (!_queueCreated)
            {
                EnsureRabbitIsConnected();
                using (var channel = _rabbitConnection.CreateModel())
                {
                    DeclareDeadletter(channel);
                    channel.QueueDeclare(queue: _rabbitOptions.QueueName, arguments: new Dictionary<string, object>
                    {
                        ["dead-letter-exchange"] = _rabbitOptions.DeadLetterName!
                    }, durable: true, autoDelete: false, exclusive: false);
                    _queueCreated = true;
                }
            }
        }

        private void EnsureRabbitIsConnected()
        {
            if (!_rabbitConnection.IsConnected)
                _rabbitConnection.TryConnect();
        }

        private void DeclareDeadletter(IModel channel)
        {
            channel.QueueDeclare(_rabbitOptions.DeadLetterName, durable: false, exclusive: false, autoDelete: false);
            channel.ExchangeDeclare(_rabbitOptions.DeadLetterName, type: ExchangeType.Topic, autoDelete: false);
        }
    }
}
