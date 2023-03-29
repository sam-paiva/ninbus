using Microsoft.Extensions.Logging;
using Moq;
using Ninbus.EventBus.RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Ninbus.EventBus.Test.Rabbit
{
    public class RabbitConsumerInitializerTest
    {
        private readonly Mock<IRabbitConnection> _rabbitConnection;
        private readonly Mock<IRabbitConsumerHandler> _rabbitConsumerHandler;
        private readonly Mock<IExchangeQueueManager> _exchangeQueueCreator;
        private readonly RabbitConsumerInitializer _sut;

        public RabbitConsumerInitializerTest()
        {
            _rabbitConnection = new Mock<IRabbitConnection>();
            _rabbitConsumerHandler = new Mock<IRabbitConsumerHandler>();
            _exchangeQueueCreator = new Mock<IExchangeQueueManager>();
            var logger = new Mock<ILogger<RabbitConsumerInitializer>>();
            _sut = new RabbitConsumerInitializer(_rabbitConnection.Object, _rabbitConsumerHandler.Object, logger.Object, _exchangeQueueCreator.Object,
                new NinbusConfiguration() { QueueName = "TestQueue"});
        }

        [Fact]
        public async Task When_Initializer_Consumers_Should_Ensure_Create_Queue_And_Exchange()
        {
            var channel = new Mock<IModel>();
            _rabbitConnection.Setup(c => c.CreateModel()).Returns(channel.Object);
            await _sut.InitializeConsumersChannelAsync();
            _exchangeQueueCreator.Verify(c => c.EnsureExchangeIsCreated(), Times.Once);
            _exchangeQueueCreator.Verify(c => c.EnsureQueueIsCreated(), Times.Once);
        }

        [Fact]
        public async Task When_Initialize_Consumers_Should_Call_Model_BasicConsume()
        {
            var channel = new Mock<IModel>();
            
            var consumer = new EventingBasicConsumer(channel.Object);
            _rabbitConnection.Setup(c => c.CreateModel()).Returns(channel.Object);
            await _sut.InitializeConsumersChannelAsync();
            channel.Verify(c => c.TxSelect(), Times.Once);
            channel.Verify(c => c.TxCommit(), Times.Once);
            channel.Verify(c => c.BasicQos(0, 1, false), Times.Once);
        }

        [Fact]
        public async Task When_Initialize_Consumers_Must_Add_Callback_Exception_Event()
        {
            var channel = new Mock<IModel>();

            var consumer = new EventingBasicConsumer(channel.Object);
            _rabbitConnection.Setup(c => c.CreateModel()).Returns(channel.Object);
            channel.SetupAdd(c => c.CallbackException += (sender, args) => { });
            await _sut.InitializeConsumersChannelAsync();

            channel.VerifyAdd(
                m => m.CallbackException += It.IsAny<EventHandler<CallbackExceptionEventArgs>>(), Times.Exactly(1));
        }

        [Fact]
        public async Task When_CallbackException_Should_Raise_Event()
        {
            var channel = new Mock<IModel>();

            var consumer = new EventingBasicConsumer(channel.Object);
            _rabbitConnection.Setup(c => c.CreateModel()).Returns(channel.Object);
            await _sut.InitializeConsumersChannelAsync();

            channel.Raise(m => m.CallbackException += null, new CallbackExceptionEventArgs(It.IsAny<Exception>()));
        }
    }
}
