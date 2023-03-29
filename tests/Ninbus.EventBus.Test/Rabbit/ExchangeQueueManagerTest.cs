using Moq;
using Ninbus.EventBus.RabbitMQ;
using RabbitMQ.Client;

namespace Ninbus.EventBus.Test.Rabbit
{
    public class ExchangeQueueManagerTest
    {
        private readonly Mock<IRabbitConnection> _rabbitConnection;
        private readonly ExchangeQueueManager _sut;

        public ExchangeQueueManagerTest()
        {
            _rabbitConnection = new Mock<IRabbitConnection>();
            _sut = new ExchangeQueueManager(_rabbitConnection.Object, new NinbusConfiguration());
        }

        [Fact]
        public void When_Try_To_Create_Queue_Should_Call_QueueDeclare()
        {
            var connection = new Mock<IConnection>();
            var model = new Mock<IModel>();
            connection.Setup(c => c.IsOpen).Returns(true);
            _rabbitConnection.Setup(c => c.CreateModel()).Returns(model.Object);
            _rabbitConnection.Setup(c => c.IsConnected).Returns(true);
            _sut.EnsureQueueIsCreated();
            _rabbitConnection.Verify(c => c.CreateModel(), Times.Once);
            model.Verify(c => c.QueueDeclare(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), false, It.IsAny<Dictionary<string, object>>()), Times.Exactly(2));
        }

        [Fact]
        public void When_Try_To_Create_Exchange_Should_Call_DeclareExchange()
        {
            var connection = new Mock<IConnection>();
            var model = new Mock<IModel>();
            connection.Setup(c => c.IsOpen).Returns(true);
            _rabbitConnection.Setup(c => c.CreateModel()).Returns(model.Object);
            _rabbitConnection.Setup(c => c.IsConnected).Returns(true);
            _sut.EnsureExchangeIsCreated();
            _rabbitConnection.Verify(c => c.CreateModel(), Times.Once);
            model.Verify(c => c.ExchangeDeclare(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), false, It.IsAny<Dictionary<string, object>>()), Times.Exactly(1));
        }
    }
}
