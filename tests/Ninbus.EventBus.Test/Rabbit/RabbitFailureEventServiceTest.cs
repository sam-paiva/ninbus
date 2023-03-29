using Microsoft.Extensions.Logging;
using Moq;
using Ninbus.EventBus.RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace Ninbus.EventBus.Test.Rabbit
{
    public class RabbitFailureEventServiceTest
    {
        private readonly Mock<ISubscriptionManager> _subscriptionManager;
        private readonly Mock<ILogger<RabbitFailureEventService>> _logger;
        private readonly RabbitFailureEventService _sut;

        public RabbitFailureEventServiceTest()
        {
            _subscriptionManager = new Mock<ISubscriptionManager>();
            var rabbitEventBusOptions = new NinbusConfiguration
            {
                QueueName = "TestQueue"
            };
            _logger = new Mock<ILogger<RabbitFailureEventService>>();
            _sut = new RabbitFailureEventService(_subscriptionManager.Object, rabbitEventBusOptions, _logger.Object);
        }

        [Fact]
        public async Task When_Discard_Event_Should_Publish_On_Dead_Letter_Queue()
        {
            var channel = new Mock<IModel>();
            var basicProps = new Mock<IBasicProperties>().Object;
            channel.Setup(c => c.CreateBasicProperties()).Returns(basicProps);
            BasicDeliverEventArgs eventArgs = new()
            {
                BasicProperties = channel.Object.CreateBasicProperties()
            };
            eventArgs.RoutingKey = nameof(EventTest);
            EventTest @event = new();
            Exception ex = new();
            
            var subscription = new Subscription<EventTest>();
            subscription.OnFailure(c => c.NeverRetry());
            _subscriptionManager.Setup(c => c.AddSubscription<EventTest>()).Returns(subscription);
            _subscriptionManager.Setup(c => c.FindSubscription(nameof(EventTest))).Returns(subscription);

            await _sut.HandleExceptionEventAsync(channel.Object, eventArgs, @event, ex);
            channel.Verify(c => c.BasicAck(It.IsAny<ulong>(), false), Times.Once);
            channel.Verify(c => c.BasicPublish(It.Is<string>(c => c.Contains(".error")), It.Is<string>(c => c.Equals(eventArgs.RoutingKey)), It.IsAny<bool>(),
            It.Is<IBasicProperties>(c => c.Equals(eventArgs.BasicProperties)), It.Is<ReadOnlyMemory<byte>>(c => c.Equals(eventArgs.Body))), Times.Once);
            channel.Verify(c => c.TxCommit(), Times.Once);
        }

        [Fact]
        public async Task When_Retry_Event_Should_Requeue_Message()
        {
            var channel = new Mock<IModel>();
            var basicProps = new Mock<IBasicProperties>().Object;
            channel.Setup(c => c.CreateBasicProperties()).Returns(basicProps);
            BasicDeliverEventArgs eventArgs = new()
            {
                BasicProperties = channel.Object.CreateBasicProperties()
            };
            eventArgs.RoutingKey = nameof(EventTest);
            EventTest @event = new();
            Exception ex = new();

            var subscription = new Subscription<EventTest>();
            subscription.OnFailure(c => c.RetryForTimes(It.IsAny<int>()));
            _subscriptionManager.Setup(c => c.AddSubscription<EventTest>()).Returns(subscription);
            _subscriptionManager.Setup(c => c.FindSubscription(nameof(EventTest))).Returns(subscription);

            await _sut.HandleExceptionEventAsync(channel.Object, eventArgs, @event, ex);
            channel.Verify(c => c.BasicAck(It.IsAny<ulong>(), false), Times.Once);
            channel.Verify(c => c.BasicPublish(It.IsAny<string>(), It.Is<string>(c => c.Equals(eventArgs.RoutingKey)), It.IsAny<bool>(),
            It.Is<IBasicProperties>(c => c.Equals(eventArgs.BasicProperties)), It.Is<ReadOnlyMemory<byte>>(c => c.Equals(eventArgs.Body))), Times.Once);
            channel.Verify(c => c.TxCommit(), Times.Once);
        }
    }
}
