using MediatR;

namespace Ninbus.EventBus
{
    public abstract class IntegrationEvent : IRequest<Result>
    {
        public Guid Id { get; }
        public DateTime CreatedAt { get; }
        public string Name { get; }

        protected IntegrationEvent()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            Name = GetType().Name;
        }
    }
}
