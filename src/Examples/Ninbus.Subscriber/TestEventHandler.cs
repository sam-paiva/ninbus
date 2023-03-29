using Ninbus.EventBus;

namespace Ninbus.Subscriber
{
    public class TestEventHandler : IIntegrationEventHandler<TestEvent>
    {
        public Task<Result> Handle(TestEvent request, CancellationToken cancellationToken)
        {
            //do your logic
            return Task.FromResult(Result.Success());
        }
    }
}
