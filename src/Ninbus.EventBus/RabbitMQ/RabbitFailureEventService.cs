using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Ninbus.EventBus.RabbitMQ
{
    public class RabbitFailureEventService : IFailureEventService
    {
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly NinbusConfiguration _rabbitEventBusOptions;
        private readonly ILogger<RabbitFailureEventService> _logger;

        public RabbitFailureEventService(ISubscriptionManager subscriptionManager,
            NinbusConfiguration options, ILogger<RabbitFailureEventService> logger)
        {
            _subscriptionManager = subscriptionManager;
            _rabbitEventBusOptions = options;
            _logger = logger;
        }

        public Task HandleExceptionEventAsync(IModel channel, BasicDeliverEventArgs eventArgs, dynamic @event, Exception ex)
        {
            ISubscription subscription = _subscriptionManager.FindSubscription(eventArgs.RoutingKey)!;
            int maxRetryTimes = subscription!.RetryPolicyConfiguration.MaxRetryTimes;
            Type? exceptionType = subscription!.RetryPolicyConfiguration.ExceptionType;
            bool sameTypeException = exceptionType is not null ? exceptionType.Equals(ex.GetType()) : false;
            bool discardEvent = subscription!.RetryPolicyConfiguration.DiscardEvent is true || sameTypeException;
            TimeSpan retryDelayTime = subscription!.RetryPolicyConfiguration.RetryInterval;
            bool foreverRetry = subscription!.RetryPolicyConfiguration.ForeverRetry;

            if (discardEvent)
            {
                _logger.LogInformation($"Event {eventArgs.RoutingKey} will be discarded. Publishing in DLQ");
                PushToQueue(channel, eventArgs, retryDelayTime, true);
                return Task.CompletedTask;
            }

            eventArgs.BasicProperties.Headers = UpdateHeaders(eventArgs);

            if (foreverRetry)
            {
                _logger.LogInformation($"Requeing event {eventArgs.RoutingKey} with id: {@event.Id}  with attempt: {GetTotalAttempts(eventArgs.BasicProperties.Headers)}");
                PushToQueue(channel, eventArgs, retryDelayTime);
            }
            else if (GetTotalAttempts(eventArgs.BasicProperties.Headers) <= maxRetryTimes)
            {
                _logger.LogInformation($"Requeing event {eventArgs.RoutingKey} with id: {@event.Id}  with attempt: {GetTotalAttempts(eventArgs.BasicProperties.Headers)}");
                PushToQueue(channel, eventArgs, retryDelayTime);
            }
            else
            {
                _logger.LogInformation($"Finishing retries of {eventArgs.RoutingKey} with id: {@event.Id} and publishing in DLQ");
                PushToQueue(channel, eventArgs, retryDelayTime, true);
            }

            return Task.CompletedTask;
        }

        private void PushToQueue(IModel channel, BasicDeliverEventArgs eventArgs, TimeSpan retryDelayTime, bool publishOnDeadLetter = false)
        {
            channel.BasicAck(eventArgs.DeliveryTag, false);

            if (publishOnDeadLetter)
                channel.BasicPublish(exchange: _rabbitEventBusOptions.DeadLetterName, eventArgs.RoutingKey, eventArgs.BasicProperties, eventArgs.Body);
            else
            {
                Thread.Sleep(retryDelayTime);
                channel.BasicPublish(eventArgs.Exchange, eventArgs.RoutingKey, eventArgs.BasicProperties, eventArgs.Body);
            }
            channel.TxCommit();
        }

        private IDictionary<string, object> UpdateHeaders(BasicDeliverEventArgs eventArgs)
        {
            var headers = eventArgs.BasicProperties.Headers;
            if (headers is null)
            {
                headers = new Dictionary<string, object>
                    {
                        { "retryAttempts", 1 }
                    };
                return headers;
            }

            headers!.TryGetValue("retryAttempts", out object? retries);

            headers.Remove("retryAttempts");
            headers.Add("retryAttempts", (int)retries! + 1);
            return headers;
        }

        private int GetTotalAttempts(IDictionary<string, object> headers)
        {
            if (headers is null)
            {
                return 1;
            }

            headers!.TryGetValue("retryAttempts", out object? retries);
            return (int)retries!;
        }
    }
}
