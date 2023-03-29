using MediatR;

namespace Ninbus.EventBus
{
    public interface IIntegrationEventHandler<TEvent> : IRequestHandler<TEvent, Result>
        where TEvent : IRequest<Result>
    {

    }
}
