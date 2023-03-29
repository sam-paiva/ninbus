using Microsoft.Extensions.Logging;
using Moq;
using Ninbus.EventBus.RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading.Channels;

namespace Ninbus.EventBus.Test.Rabbit
{
    public class RabbitConnectionTest
    {
        private readonly Mock<ILogger<NinbusConfiguration>> _logger;
        private readonly Mock<IConnectionFactory> _connectionFactory;
        private readonly RabbitConnection _sut;

        public RabbitConnectionTest()
        {
            _logger = new Mock<ILogger<NinbusConfiguration>>();
            _connectionFactory = new Mock<IConnectionFactory>();
            _sut = new RabbitConnection(_logger.Object, _connectionFactory.Object);
        }

        [Fact]
        public void Connection_Should_Be_True_After_Try_To_Connect_To_Rabbit()
        {
            var connection = new Mock<IConnection>();
            connection.Setup(c => c.IsOpen).Returns(true);
            _connectionFactory.Setup(c => c.CreateConnection()).Returns(connection.Object);
            _sut.TryConnect();
            Assert.True(_sut.IsConnected);
        }

        [Fact]
        public void Should_Throws_FailedToConnectToRabbitException_When_Cant_Connect_To_Rabbit()
        {
            var connection = new Mock<IConnection>();
            connection.Setup(c => c.IsOpen).Returns(false);
            _connectionFactory.Setup(c => c.CreateConnection()).Returns(connection.Object);
            Assert.False(_sut.IsConnected);
            Assert.Throws<FailedToConnectToRabbitException>(() => _sut.TryConnect());
        }

        [Fact]
        public void Should_Return_IModel_When_Call_Create_Model()
        {
            var connection = new Mock<IConnection>();
            var model = new Mock<IModel>();
            connection.Setup(c => c.IsOpen).Returns(true);
            connection.Setup(c => c.CreateModel()).Returns(model.Object);
            _connectionFactory.Setup(c => c.CreateConnection()).Returns(connection.Object);
            _sut.TryConnect();
            var modelResult = _sut.CreateModel();
            Assert.IsAssignableFrom<IModel>(modelResult);
        }

        [Fact]
        public void Should_Throws_InvalidOperationException_When_Call_Create_Model_And_Rabbit_Is_Not_Connected()
        {
            var connection = new Mock<IConnection>();
            var model = new Mock<IModel>();
            connection.Setup(c => c.IsOpen).Returns(false);
            connection.Setup(c => c.CreateModel()).Returns(model.Object);
            _connectionFactory.Setup(c => c.CreateConnection()).Returns(connection.Object);
            Assert.Throws<InvalidOperationException>(() => _sut.CreateModel());
        }

        [Fact]
        public void When_Dispose_Should_Call_Connection_Dipose()
        {
            var connection = new Mock<IConnection>();
            var model = new Mock<IModel>();
            connection.Setup(c => c.IsOpen).Returns(true);
            connection.Setup(c => c.CreateModel()).Returns(model.Object);
            _connectionFactory.Setup(c => c.CreateConnection()).Returns(connection.Object);
            _sut.TryConnect();
            _sut.Dispose();

            connection.Verify(c => c.Dispose(), Times.Once);
        }

        [Fact]
        public void When_Dispose_Throws_Exception_Should_Log_Critical()
        {
            var connection = new Mock<IConnection>();
            var model = new Mock<IModel>();
            var exception = new Exception();
            connection.Setup(c => c.IsOpen).Returns(true);
            connection.Setup(c => c.CreateModel()).Returns(model.Object);
            connection.Setup(c => c.Dispose()).Throws(exception);
            _connectionFactory.Setup(c => c.CreateConnection()).Returns(connection.Object);
            _sut.TryConnect();
            _sut.Dispose();

            connection.Verify(c => c.Dispose(), Times.Once);
            _logger.Verify(
                x => x.Log(
                        LogLevel.Critical,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => true),
                        It.IsAny<Exception>(),
                        It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)));
        }

        [Fact]
        public void Connection_Start_Event_Listeners_When_Start_Rabbit()
        {
            var connection = new Mock<IConnection>();
            connection.Setup(c => c.IsOpen).Returns(true);
            connection.SetupAdd(c => c.ConnectionShutdown += (sender, args) => { });
            connection.SetupAdd(c => c.CallbackException += (sender, args) => { });
            connection.SetupAdd(c => c.ConnectionBlocked += (sender, args) => { });
            _connectionFactory.Setup(c => c.CreateConnection()).Returns(connection.Object);
            _sut.TryConnect();

            connection.VerifyAdd(
                m => m.ConnectionShutdown += It.IsAny<EventHandler<ShutdownEventArgs>>(), Times.Exactly(1));
            connection.VerifyAdd(
                m => m.CallbackException += It.IsAny<EventHandler<CallbackExceptionEventArgs>>(), Times.Exactly(1));
            connection.VerifyAdd(
                m => m.ConnectionBlocked += It.IsAny<EventHandler<ConnectionBlockedEventArgs>>(), Times.Exactly(1));
        }

        [Fact]
        public void Raise_Events_When_Try_To_Connect()
        {
            var connection = new Mock<IConnection>();
            connection.Setup(c => c.IsOpen).Returns(true);
            _connectionFactory.Setup(c => c.CreateConnection()).Returns(connection.Object);
            _sut.TryConnect();

            connection.Raise(m => m.ConnectionShutdown += null, It.IsAny<ShutdownEventArgs>());
            connection.Raise(m => m.CallbackException += null, It.IsAny<CallbackExceptionEventArgs>());
            connection.Raise(m => m.ConnectionBlocked += null, It.IsAny<ConnectionBlockedEventArgs>());
        }
    }
}
