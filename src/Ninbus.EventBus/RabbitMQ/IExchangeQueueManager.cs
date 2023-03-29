namespace Ninbus.EventBus.RabbitMQ
{
    public interface IExchangeQueueManager
    {
        void EnsureQueueIsCreated();
        void EnsureExchangeIsCreated();
    }
}
