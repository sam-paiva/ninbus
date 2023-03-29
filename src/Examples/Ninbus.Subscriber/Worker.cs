using Ninbus.EventBus;

namespace Ninbus.Subscriber
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private bool _subscribed = false;

        public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!_subscribed)
                {
                    var scope = _scopeFactory.CreateScope();
                    var eventSubscriber = scope.ServiceProvider.GetService<IEventSubscriber>()!;
                    eventSubscriber.Subscribe<TestEvent>();
                    await eventSubscriber.StartListeningAsync();

                    _subscribed = true;
                }
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}