# Ninbus

Ninbus is an event bus implementation using .NET and RabbitMQ

## How to use

After installing the package **Ninbus.EventBus** via Nuget,
you need to inject the library by dependency injection.

In the following examples, the .NET template **Worker Service** is being used.
See the example below:

### Publisher

#### TestEvent.cs

```c#
using Ninbus.EventBus;

namespace Ninbus.Publisher
{
    public class TestEvent : IntegrationEvent
    {
        public string? Message { get; set; }
    }
}

```

#### Program.cs

```c#
using Ninbus.EventBus;
using Ninbus.EventBus.IoC;
using Ninbus.Publisher;
using System.Reflection;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        NinbusConfiguration configuration = new()
        {
            HostName = "your-host-name-url",
            VirtualHost = "your-virtual-host",
            Password = "your-password",
            Port = 5672, //default-port,
            Username = "your-username",
            ExchangeName = "TestExchange"

        };
        services.UseNinbus(configuration, Assembly.GetExecutingAssembly());
    })
    .Build();

await host.RunAsync();
```

#### Worker.cs

```c#
using Ninbus.EventBus;

namespace Ninbus.Publisher
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var scope = _scopeFactory.CreateScope();
                var eventPublisher = scope.ServiceProvider.GetService<IEventPublisher>()!;
                var @event = new TestEvent { Message = "Hello World" };
                await eventPublisher.Publish(@event); //Using the publish method to publish the event.

                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
```

### Consumer

For consumers, basically you need to do the same things as for publisher, with just a few differences. See the example below:

#### Program.cs

```c#
using Ninbus.EventBus;
using Ninbus.EventBus.IoC;
using Ninbus.Subscriber;
using System.Reflection;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        NinbusConfiguration configuration = new()
        {
            HostName = "your-host-name-url",
            VirtualHost = "your-virtual-host",
            Password = "your-password",
            Port = 5672, //default-port,
            Username = "your-username",
            ExchangeName = "TestExchange",
            QueueName = "TestQueue"

        };
        services.UseNinbus(configuration, Assembly.GetExecutingAssembly());
    })
    .Build();

await host.RunAsync();
```

#### TestEvent.cs

Remember to always use the same class name as the publisher event. The properties must be the same and the setter must always be public. The success of event deserialization depends on these factors.

```c#
using Ninbus.EventBus;

namespace Ninbus.Subscriber
{
    public class TestEvent : IntegrationEvent
    {
        public string? Message { get; set; }
    }
}
```

#### TestEventHandler.cs

The event handler must inherit from the **IIntegrationEventHandler.cs** interface, which use the generic type **IntegrationEvent.cs**. See below:

```c#
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
```

#### Worker.cs

The Consumer just need to call the **Subscribe** method passing the event as a generic type. After subscribing all events, the **StartListeningAsync** method must be called.

```c#
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
```

## Retry Policies

You can also configure retry policies in case there is any failure in handling the event.
The **OnFailure** method receives a configuration of how the exceptions that occur will be treated. See below:

### RetryForTimes

Attempt to process the event 5 times before discarding the event and posting it to the dead-letter queue

```c#
  var scope = _scopeFactory.CreateScope();
  var eventSubscriber = scope.ServiceProvider.GetService<IEventSubscriber>()!;
  eventSubscriber.Subscribe<TestEvent>().OnFailure(c => c.RetryForTimes(5));
```

### SetIntervalTime

Attempt to process the event 5 times before discarding the event and posting it to the dead-letter queue, and waiting 5 seconds before retrying to process the event

```c#
  var scope = _scopeFactory.CreateScope();
  var eventSubscriber = scope.ServiceProvider.GetService<IEventSubscriber>()!;
  eventSubscriber.Subscribe<TestEvent>().OnFailure(c =>
    c.RetryForTimes(5).SetIntervalTime(TimeSpan.FromSeconds(5)));
```

### RetryForever

It will process the event forever even in case of failures

```c#
  var scope = _scopeFactory.CreateScope();
  var eventSubscriber = scope.ServiceProvider.GetService<IEventSubscriber>()!;
  eventSubscriber.Subscribe<TestEvent>().OnFailure(c => c.RetryForever());
```

### NeverRetry

Discard the event and publish it to the dead-letter queue on failure

```c#
  var scope = _scopeFactory.CreateScope();
  var eventSubscriber = scope.ServiceProvider.GetService<IEventSubscriber>()!;
  eventSubscriber.Subscribe<TestEvent>().OnFailure(c => c.NeverRetry());
```

### ShouldDiscard

Discard the event and post it to the dead-letter queue if the expected exception occurs

```c#
  var scope = _scopeFactory.CreateScope();
  var eventSubscriber = scope.ServiceProvider.GetService<IEventSubscriber>()!;
  eventSubscriber.Subscribe<TestEvent>().OnFailure(c => c.ShouldDiscard<CustomException>());
```
