using Ninbus.EventBus;

namespace Ninbus.Subscriber
{
    public class TestEventHandler : IIntegrationEventHandler<TestEvent>
    {
        public Task<Result> Handle(TestEvent request, CancellationToken cancellationToken)
        {
            try
            {
                return Task.FromResult(Result.Success());
            }
            catch (Exception ex)
            {
                return Task.FromResult(Result.Error(ex));
            }
        }
    }
}
