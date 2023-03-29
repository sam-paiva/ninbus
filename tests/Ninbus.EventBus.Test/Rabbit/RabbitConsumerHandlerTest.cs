using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Ninbus.EventBus.RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Ninbus.EventBus.Test.Rabbit
{
    public class RabbitConsumerHandlerTest
    {
        private readonly RabbitConsumerHandler _sut;
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactory;
        private readonly Mock<ISubscriptionManager> _subsManager;
        private readonly Mock<ILogger<RabbitConsumerHandler>> _logger;
        public RabbitConsumerHandlerTest()
        {
            _serviceScopeFactory = new Mock<IServiceScopeFactory>();
            _subsManager = new Mock<ISubscriptionManager>();
            _logger = new Mock<ILogger<RabbitConsumerHandler>>();
            _sut = new RabbitConsumerHandler(_serviceScopeFactory.Object, _subsManager.Object, _logger.Object);
        }

        [Fact]
        public async Task When_Handle_Event_Sucess_Should_Call_BasicAck_And_Commit()
        {
            var serviceProvider = new Mock<IServiceProvider>();
            var mediator = new Mock<IMediator>();
            var channel = new Mock<IModel>();
            var basicProperties = new Mock<IBasicProperties>();
            var @event = new EventTest();
            basicProperties.Setup(c => c.CorrelationId).Returns(Guid.NewGuid().ToString());
            var scope = new Mock<IServiceScope>();

            var eventArgs = new BasicDeliverEventArgs()
            {
                RoutingKey = nameof(EventTest),
                BasicProperties = basicProperties.Object,
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event))
            };

            _subsManager.Setup(c => c.FindSubscription(nameof(EventTest))).Returns(new Subscription<EventTest>());
            _serviceScopeFactory!.Setup(c => c.CreateScope()).Returns(scope.Object);
            scope.Setup(c => c.ServiceProvider).Returns(serviceProvider.Object);
            serviceProvider.Setup(c => c.GetService(typeof(IMediator))).Returns(mediator.Object);
            mediator.Setup(c => c.Send(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
            await _sut.HandleAsync(channel.Object, eventArgs);

            channel.Verify(c => c.BasicAck(It.IsAny<ulong>(), false), Times.Once);
            channel.Verify(c => c.TxCommit(), Times.Once);
        }

        [Fact]
        public async Task If_Throws_Exception_Should_Handle_Exception()
        {
            var serviceProvider = new Mock<IServiceProvider>();
            var mediator = new Mock<IMediator>();
            var channel = new Mock<IModel>();
            var basicProperties = new Mock<IBasicProperties>();
            var @event = new EventTest();
            basicProperties.Setup(c => c.CorrelationId).Returns(Guid.NewGuid().ToString());
            var scope = new Mock<IServiceScope>();
            var failureEventService = new Mock<IFailureEventService>();

            var eventArgs = new BasicDeliverEventArgs()
            {
                RoutingKey = nameof(EventTest),
                BasicProperties = basicProperties.Object,
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event))
            };

            _subsManager.Setup(c => c.FindSubscription(nameof(EventTest))).Returns(new Subscription<EventTest>());
            _serviceScopeFactory!.Setup(c => c.CreateScope()).Returns(scope.Object);
            scope.Setup(c => c.ServiceProvider).Returns(serviceProvider.Object);
            serviceProvider.Setup(c => c.GetService(typeof(IMediator))).Returns(mediator.Object);
            serviceProvider.Setup(c => c.GetService(typeof(IFailureEventService))).Returns(failureEventService.Object);
            mediator.Setup(c => c.Send(It.IsAny<object>(), It.IsAny<CancellationToken>())).Throws<Exception>();
            await _sut.HandleAsync(channel.Object, eventArgs);

            failureEventService.Verify(c => c.HandleExceptionEventAsync(channel.Object, eventArgs, It.IsAny<object>(), It.IsAny<Exception>()), Times.Once);
        }

        [Fact]
        public async Task If_Throws_Exception_Should_Log_Error()
        {
            var channel = new Mock<IModel>();
            var basicProperties = new Mock<IBasicProperties>();
            var @event = new EventTest();
            basicProperties.Setup(c => c.CorrelationId).Returns(Guid.NewGuid().ToString());

            var eventArgs = new BasicDeliverEventArgs()
            {
                RoutingKey = nameof(EventTest),
                BasicProperties = basicProperties.Object,
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event))
            };

            _subsManager.Setup(c => c.FindSubscription(nameof(EventTest))).Throws<Exception>();

            await _sut.HandleAsync(channel.Object, eventArgs);

            _logger.Verify(
               x => x.Log(
                       LogLevel.Error,
                       It.IsAny<EventId>(),
                       It.Is<It.IsAnyType>((v, t) => true),
                       It.IsAny<Exception>(),
                       It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)));
        }

        [Fact]
        public async Task When_Result_Event_Handler_Is_Not_Success_Should_Call_Failure_Event_Service()
        {
            var serviceProvider = new Mock<IServiceProvider>();
            var mediator = new Mock<IMediator>();
            var channel = new Mock<IModel>();
            var basicProperties = new Mock<IBasicProperties>();
            var @event = new EventTest();
            basicProperties.Setup(c => c.CorrelationId).Returns(Guid.NewGuid().ToString());
            var scope = new Mock<IServiceScope>();
            var failureEventService = new Mock<IFailureEventService>();

            var eventArgs = new BasicDeliverEventArgs()
            {
                RoutingKey = nameof(EventTest),
                BasicProperties = basicProperties.Object,
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event))
            };

            _subsManager.Setup(c => c.FindSubscription(nameof(EventTest))).Returns(new Subscription<EventTest>());
            _serviceScopeFactory!.Setup(c => c.CreateScope()).Returns(scope.Object);
            scope.Setup(c => c.ServiceProvider).Returns(serviceProvider.Object);
            serviceProvider.Setup(c => c.GetService(typeof(IMediator))).Returns(mediator.Object);
            serviceProvider.Setup(c => c.GetService(typeof(IFailureEventService))).Returns(failureEventService.Object);
            mediator.Setup(c => c.Send(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Error(new Exception()));

            await _sut.HandleAsync(channel.Object, eventArgs);

            failureEventService.Verify(c => c.HandleExceptionEventAsync(channel.Object, eventArgs, It.IsAny<object>(), It.IsAny<Exception>()), Times.Once);
        }
    }
}
