using Ninbus.EventBus;

namespace Ninbus.Subscriber
{
    public class TestEvent : IntegrationEvent
    {
        public string? Message { get; set; }
    }
}
