using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ninbus.EventBus.RabbitMQ
{
    public sealed class RabbitConsumerInitializer : IRabbitConsumerInitializer
    {
        private readonly IRabbitConnection _rabbitConnection;
        private readonly IRabbitConsumerHandler _rabbitConsumerHandler;
        private readonly ILogger<RabbitConsumerInitializer> _logger;
        private readonly IExchangeQueueManager _exchangeQueueCreator;
        private readonly NinbusConfiguration _options;
        private readonly IList<IModel> _channels = new List<IModel>();

        public RabbitConsumerInitializer(IRabbitConnection rabbitConnection, IRabbitConsumerHandler rabbitConsumerHandler, ILogger<RabbitConsumerInitializer> logger,
            IExchangeQueueManager exchangeQueueCreator, NinbusConfiguration options)
        {
            _rabbitConnection = rabbitConnection;
            _rabbitConsumerHandler = rabbitConsumerHandler;
            _logger = logger;
            _exchangeQueueCreator = exchangeQueueCreator;
            _options = options;
        }

        public async Task InitializeConsumersChannelAsync()
        {
            _exchangeQueueCreator.EnsureExchangeIsCreated();
            _exchangeQueueCreator.EnsureQueueIsCreated();

            _logger.LogInformation("Initilizing consumers");

            var consumerStarts = new List<Task>();
            for (int i = 0; i < _options.ConsumersCount; i++)
            {
                consumerStarts.Add(Task.Run(() => InitializeConsumers()));
            }

            await Task.WhenAll(consumerStarts);
        }

        private void InitializeConsumers()
        {
            var channel = _rabbitConnection.CreateModel();
            _channels.Add(channel);
            channel.BasicQos(0, 1, false);
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (sender, ea) => _rabbitConsumerHandler.HandleAsync(channel, ea);

            channel.CallbackException += (sender, ea) =>
            {
                if (channel.IsOpen)
                    channel.Dispose();
                _channels.Remove(channel);
                InitializeConsumers();
            };

            channel.TxSelect();
            channel.TxCommit();
            channel.BasicConsume(_options.QueueName, autoAck: false, consumer);
            _logger.LogInformation("Consumer Initialized successfully");
        }
    }
}
