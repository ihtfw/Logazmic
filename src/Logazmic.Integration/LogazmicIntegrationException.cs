namespace Logazmic.Integration
{
    using System;

    public class LogazmicIntegrationException : Exception
    {
        public LogazmicIntegrationException(string message)
            : base(message)
        {
        }

        public LogazmicIntegrationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}