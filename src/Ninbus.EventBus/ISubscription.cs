using System;

namespace Ninbus.EventBus
{
    public interface ISubscription
    {
        Type EventType { get; }
        string EventName { get; }
        RetryPolicyConfiguration RetryPolicyConfiguration { get; }
    }
}