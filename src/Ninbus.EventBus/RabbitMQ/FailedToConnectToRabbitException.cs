using System;
using System.Runtime.Serialization;

namespace Ninbus.EventBus.RabbitMQ
{
    [Serializable]
    internal class FailedToConnectToRabbitException : Exception
    {
        public FailedToConnectToRabbitException()
        {
        }

        public FailedToConnectToRabbitException(string message) : base(message)
        {
        }

        public FailedToConnectToRabbitException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected FailedToConnectToRabbitException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}