namespace Ninbus.EventBus.RabbitMQ
{
    public interface IRabbitConsumerInitializer
    {
        Task InitializeConsumersChannelAsync();
    }
}
