using Ninbus.EventBus;

namespace Ninbus.Publisher
{
    public class TestEvent : IntegrationEvent
    {
        public string? Message { get; set; }
    }
}
