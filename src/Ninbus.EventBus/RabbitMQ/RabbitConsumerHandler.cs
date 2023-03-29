using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Ninbus.EventBus.RabbitMQ
{
    public class RabbitConsumerHandler : IRabbitConsumerHandler
    {
        private readonly IServiceScopeFactory? _serviceScopeFactory;
        private readonly ISubscriptionManager _subsManager;
        private readonly ILogger<RabbitConsumerHandler> _logger;

        public RabbitConsumerHandler(IServiceScopeFactory serviceScopeFactory, ISubscriptionManager subsManager, ILogger<RabbitConsumerHandler> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _subsManager = subsManager;
            _logger = logger;
        }

        public async Task HandleAsync(IModel consumerChannel, BasicDeliverEventArgs e)
        {
            using var scope = _serviceScopeFactory!.CreateScope();
            var eventId = e.BasicProperties.CorrelationId;
            var eventName = e.RoutingKey;

            try
            {
                _logger.LogInformation("A new event Arrived");
                if (TryRetrieveEventType(eventName, out Type eventType) && TryDeserializeEvent(e, eventType, out object? @event))
                {
                    await TryHandleEventAsync(consumerChannel, e, scope, @event!);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to proccess event");
            }

        }

        private async Task TryHandleEventAsync(IModel consumerChannel, BasicDeliverEventArgs e, IServiceScope scope, object @event)
        {
            var serviceProvider = scope.ServiceProvider;
            var failureEventService = serviceProvider.GetService<IFailureEventService>()!;
            var mediator = serviceProvider.GetService<IMediator>()!;

            try
            {
                dynamic? result = await mediator.Send(@event, default);

                if (result!.IsSuccess)
                {
                    _logger.LogInformation("Event was successfully handled");
                    consumerChannel.BasicAck(e.DeliveryTag, false);
                    consumerChannel.TxCommit();
                }
                else
                {
                    _logger.LogWarning("Failed to handled event", result.Exception as Exception);
                    await failureEventService.HandleExceptionEventAsync(consumerChannel, e, @event, result.Exception);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle event");
                await failureEventService.HandleExceptionEventAsync(consumerChannel, e, @event, ex);
            }
        }

        private bool TryDeserializeEvent(BasicDeliverEventArgs args, Type eventType, out object? @event)
        {
            _logger.LogInformation("Trying to deserialize event");
            var message = Encoding.UTF8.GetString(args.Body.ToArray());

            try
            {
                @event = JsonConvert.DeserializeObject(message, eventType);
                _logger.LogInformation("Event was deserialized");
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Failed to deserialize event");
                @event = null;
                return false;
            }

            return true;
        }

        private bool TryRetrieveEventType(string eventName, out Type eventType)
        {
            _logger.LogInformation("Trying to find event susbscription");

            var eventSubscription = _subsManager.FindSubscription(eventName);
            eventType = eventSubscription?.EventType!;

            return eventType != null;
        }
    }
}
