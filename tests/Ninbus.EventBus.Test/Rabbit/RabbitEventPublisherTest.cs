using Microsoft.Extensions.Logging;
using Moq;
using Ninbus.EventBus.RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Ninbus.EventBus.Test.Rabbit
{
    public class RabbitEventPublisherTest
    {
        private readonly Mock<IRabbitConnection> _rabbitConnection;
        private readonly Mock<IExchangeQueueManager> _exchangeQueueCreator;
        private readonly RabbitEventPublisher _sut;

        public RabbitEventPublisherTest()
        {
            _rabbitConnection = new Mock<IRabbitConnection>();
            _exchangeQueueCreator = new Mock<IExchangeQueueManager>();
            _sut = new RabbitEventPublisher(_rabbitConnection.Object, _exchangeQueueCreator.Object, new NinbusConfiguration(), new Mock<ILogger<RabbitEventPublisher>>().Object);
        }

        [Fact]
        public async Task When_Call_Publish_Should_Push_Message_In_Queue()
        {
            var channel = new Mock<IModel>();
            var basicProps = new Mock<IBasicProperties>().Object;
            channel.Setup(c => c.CreateBasicProperties()).Returns(basicProps);
            BasicDeliverEventArgs eventArgs = new()
            {
                BasicProperties = channel.Object.CreateBasicProperties()
            };
            _rabbitConnection.Setup(c => c.CreateModel()).Returns(channel.Object);
            eventArgs.RoutingKey = nameof(EventTest);
            await _sut.Publish(new EventTest());
            _exchangeQueueCreator.Verify(c => c.EnsureExchangeIsCreated(), Times.Once);
            _rabbitConnection.Verify(c => c.CreateModel(), Times.Once);
            channel.Verify(c => c.BasicPublish(It.IsAny<string>(), It.Is<string>(c => c.Equals(eventArgs.RoutingKey)), It.Is<bool>(c => true),
                It.IsAny<IBasicProperties>(), It.IsAny<ReadOnlyMemory<byte>>()), Times.Once);
        }

        [Fact]
        public async Task If_Not_Connected_To_Rabbit_Should_Call_Try_Connect()
        {
            var channel = new Mock<IModel>();
            var basicProps = new Mock<IBasicProperties>().Object;
            channel.Setup(c => c.CreateBasicProperties()).Returns(basicProps);
            BasicDeliverEventArgs eventArgs = new()
            {
                BasicProperties = channel.Object.CreateBasicProperties()
            };
            _rabbitConnection.Setup(c => c.CreateModel()).Returns(channel.Object);
            eventArgs.RoutingKey = nameof(EventTest);
            _rabbitConnection.Setup(c => c.IsConnected).Returns(false);
            await _sut.Publish(new EventTest());
            _rabbitConnection.Verify(c => c.TryConnect(), Times.Once);   
        }

        [Fact]
        public async Task If_Connected_To_Rabbit_Should_Not_Call_Try_Connect()
        {
            var channel = new Mock<IModel>();
            var basicProps = new Mock<IBasicProperties>().Object;
            channel.Setup(c => c.CreateBasicProperties()).Returns(basicProps);
            BasicDeliverEventArgs eventArgs = new()
            {
                BasicProperties = channel.Object.CreateBasicProperties()
            };
            _rabbitConnection.Setup(c => c.CreateModel()).Returns(channel.Object);
            eventArgs.RoutingKey = nameof(EventTest);
            _rabbitConnection.Setup(c => c.IsConnected).Returns(true);
            await _sut.Publish(new EventTest());
            _rabbitConnection.Verify(c => c.TryConnect(), Times.Never);
        }
    }
}
