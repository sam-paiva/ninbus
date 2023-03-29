namespace Ninbus.EventBus
{
    public interface ISubscriptionManager
    {
        Subscription<T> AddSubscription<T>() where T : IntegrationEvent;
        Subscription<T> FindSubscription<T>() where T : IntegrationEvent;
        ISubscription? FindSubscription(string eventName);
    }
}
