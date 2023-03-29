namespace Ninbus.EventBus
{
    public class NinbusConfiguration
    {
        public virtual string? Username { get; set; }
        public virtual string? Password { get; set; }
        public virtual int Port { get; set; }
        public virtual string? HostName { get; set; }
        public virtual string? ExchangeName { get; set; }
        public virtual string? QueueName { get; set; }
        public virtual string? DeadLetterName => $"{QueueName}.error";
        public virtual string? VirtualHost { get; set; }
        public virtual int ConsumersCount { get; set; } = 1;
    }
}
