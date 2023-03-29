using Microsoft.Extensions.Logging;
using Moq;
using Ninbus.EventBus.RabbitMQ;
using RabbitMQ.Client;

namespace Ninbus.EventBus.Test.Rabbit
{
    public class RabbitEventSubscriberTest
    {
        private readonly RabbitEventSubscriber _sut;
        private readonly Mock<ISubscriptionManager> _subsManager;
        private readonly Mock<IRabbitConnection> _rabbitConnection;
        private readonly Mock<IRabbitConsumerInitializer> _rabbitConsumerInitializer;
        private readonly Mock<IExchangeQueueManager> _exchangeQueueCreator;

        public RabbitEventSubscriberTest()
        {
            _subsManager = new Mock<ISubscriptionManager>();
            _rabbitConnection = new Mock<IRabbitConnection>();
            _rabbitConsumerInitializer = new Mock<IRabbitConsumerInitializer>();
            _exchangeQueueCreator = new Mock<IExchangeQueueManager>();
            _sut = new RabbitEventSubscriber(_subsManager.Object, _rabbitConnection.Object, 
                _rabbitConsumerInitializer.Object, new Mock<ILogger<RabbitEventSubscriber>>().Object, _exchangeQueueCreator.Object,
                new NinbusConfiguration());
        }

        [Fact]
        public async Task When_Start_To_Listening_Should_Initializer_Consumers_Once()
        {
            await _sut.StartListeningAsync();
            _rabbitConsumerInitializer.Verify(c => c.InitializeConsumersChannelAsync(), Times.Once);
        }

        [Fact]
        public void When_Subscrive_Should_Bind_Queue()
        {
            var model = new Mock<IModel>();
            _rabbitConnection.Setup(c => c.CreateModel()).Returns(model.Object);
            _subsManager.Setup(c => c.AddSubscription<EventTest>()).Returns(new Subscription<EventTest>());
            _sut.Subscribe<EventTest>();

            _subsManager.Verify(c => c.AddSubscription<EventTest>(), Times.Once);
            _exchangeQueueCreator.Verify(c => c.EnsureQueueIsCreated(), Times.Once);
            _exchangeQueueCreator.Verify(c => c.EnsureExchangeIsCreated(), Times.Once);
            _rabbitConnection.Verify(c => c.CreateModel(), Times.Once);
            model.Verify(c => c.QueueBind(It.IsAny<string>(), It.IsAny<string>(), nameof(EventTest), null), Times.Exactly(2));
        }

        [Fact]
        public void When_Subscrive_Should_Return_Subscription()
        {
            var model = new Mock<IModel>();
            _rabbitConnection.Setup(c => c.CreateModel()).Returns(model.Object);
            _subsManager.Setup(c => c.AddSubscription<EventTest>()).Returns(new Subscription<EventTest>());
            var subscription = _sut.Subscribe<EventTest>();

            Assert.IsType<Subscription<EventTest>>(subscription);
        }
    }
}
