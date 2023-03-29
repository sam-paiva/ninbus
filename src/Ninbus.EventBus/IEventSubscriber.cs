using System.Threading.Tasks;

namespace Ninbus.EventBus
{
    public interface IEventSubscriber
    {
        Subscription<T> Subscribe<T>() where T : IntegrationEvent;
        Task StartListeningAsync();
    }
}
