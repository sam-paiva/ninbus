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
