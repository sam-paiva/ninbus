using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Ninbus.EventBus.RabbitMQ
{
    public interface IRabbitConsumerHandler
    {
        Task HandleAsync(IModel consumerChannel, BasicDeliverEventArgs e);
    }
}