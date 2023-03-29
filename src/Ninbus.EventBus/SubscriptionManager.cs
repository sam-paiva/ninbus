namespace Ninbus.EventBus
{
    public class SubscriptionManager : ISubscriptionManager
    {
        private readonly List<ISubscription> _subscriptions;

        public SubscriptionManager()
        {
            _subscriptions = new List<ISubscription>();
        }

        public Subscription<T> AddSubscription<T>() where T : IntegrationEvent
        {
            var subscription = new Subscription<T>();
            _subscriptions.Add(subscription);
            return subscription;
        }

        public Subscription<T> FindSubscription<T>() where T : IntegrationEvent
        {
            return _subscriptions.OfType<Subscription<T>>().FirstOrDefault(x => x.EventName == typeof(T).Name)!;
        }

        public ISubscription? FindSubscription(string eventName)
        {
            return _subscriptions.FirstOrDefault(s => s.EventName == eventName)!;
        }
    }
}
