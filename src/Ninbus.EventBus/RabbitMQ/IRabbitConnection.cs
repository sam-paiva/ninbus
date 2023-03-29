using RabbitMQ.Client;

namespace Ninbus.EventBus.RabbitMQ
{
    public interface IRabbitConnection : IDisposable
    {
        bool IsConnected { get; }
        void TryConnect();
        IModel CreateModel();
    }
}
