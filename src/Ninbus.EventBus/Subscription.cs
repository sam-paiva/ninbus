using System;

namespace Ninbus.EventBus
{
    public class Subscription<T> : ISubscription where T : IntegrationEvent
    {
        private Type _eventType;
        private RetryPolicyConfiguration _retryPolicyConfiguration;
        public Type EventType => _eventType;
        public string EventName => _eventType.Name;
        public RetryPolicyConfiguration RetryPolicyConfiguration => _retryPolicyConfiguration;

        public Subscription()
        {
            _retryPolicyConfiguration = new RetryPolicyConfiguration();
            _eventType = typeof(T);
        }

        public virtual void OnFailure(Action<RetryPolicyConfiguration> config) => config(RetryPolicyConfiguration);
    }
}
