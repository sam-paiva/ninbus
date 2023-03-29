using System.Threading.Tasks;

namespace Ninbus.EventBus
{
    public interface IEventPublisher
    {
        Task Publish<T>(T @event) where T : IntegrationEvent;
    }
}
