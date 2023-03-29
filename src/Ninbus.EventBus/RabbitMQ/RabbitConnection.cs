using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Ninbus.EventBus.RabbitMQ
{
    public class RabbitConnection : IRabbitConnection
    {
        private readonly ILogger<NinbusConfiguration> _logger;
        private readonly IConnectionFactory _connectionFactory;
        private IConnection? _connection;
        object sync_root = new object();
        private bool _disposed;

        public RabbitConnection(ILogger<NinbusConfiguration> logger, IConnectionFactory connectionFactory)
        {
            _logger = logger;
            _connectionFactory = connectionFactory;
        }

        public bool IsConnected => _connection?.IsOpen ?? false && !_disposed;

        public IModel CreateModel()
        {
            _logger.LogInformation("Creating RabbitMq model");
            if (!IsConnected)
                throw new InvalidOperationException();

            var model = _connection!.CreateModel();
            _logger.LogInformation("Model Created");
            return model;
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            try
            {
                _connection!.Dispose();
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Failed to dispose Rabbit Connection");
            }
        }

        public void TryConnect()
        {
            _logger.LogInformation("Trying to connect to Rabbit");
            lock (sync_root)
            {
                try
                {
                    _connection = _connectionFactory.CreateConnection();

                    if (IsConnected)
                    {
                        _logger.LogInformation("RabbitMQ Connected");
                        _connection.ConnectionShutdown += OnConnectionShutDown!;
                        _connection.CallbackException += OnCallbackException!;
                        _connection.ConnectionBlocked += OnConnectionBlocked!;
                    }
                    else
                    {
                        _logger.LogError("failed to connect to Rabbit");
                        throw new FailedToConnectToRabbitException();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("failed to connect to Rabbit");
                    throw new FailedToConnectToRabbitException(ex.Message);
                }
            }
        }

        private void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            if (_disposed) return;

            _logger.LogWarning("RabbitMQ Connecttion throw exception. Trying to re-connect...");

            TryConnect();
        }

        private void OnConnectionShutDown(object sender, ShutdownEventArgs e)
        {
            if (_disposed) return;

            _logger.LogWarning("RabbitMQ Connecttion is shudown. Trying to re-connect...");

            TryConnect();
        }
        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            if (_disposed) return;

            _logger.LogWarning("RabbitMQ Connecttion is shudown. Trying to re-connect...");

            TryConnect();
        }
    }
}
