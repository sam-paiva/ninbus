using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Ninbus.EventBus.RabbitMQ
{
    public interface IFailureEventService
    {
        Task HandleExceptionEventAsync(IModel consumerChannel, BasicDeliverEventArgs eventArgs, dynamic @event, Exception ex);
    }
}