using System;

namespace Microsoft.Extensions.Logging.Log4Net.AspNetCore.Entities
{
    internal class MessageCandidate
    {
        public MessageCandidate(LogLevel logLevel, object message, Exception exception)
        {
            LogLevel = logLevel;
            Message = message;
            Exception = exception;
        }

        public LogLevel LogLevel { get; }

        public object Message { get; }

        public Exception Exception { get; }

        public bool IsValid()
        {
            if (Message is string text)
            {
                return !string.IsNullOrEmpty(text);
            }

            return Message != null || Exception != null;
        }
    }
}