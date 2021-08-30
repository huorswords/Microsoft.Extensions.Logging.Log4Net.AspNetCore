using System;

namespace Microsoft.Extensions.Logging.Log4Net.AspNetCore.Entities
{
    /// <summary>
    /// Represents a candidate for a log message that should be printed. This candidate will either be accepted or denied by the logger that is trying to print it.
    /// </summary>
    /// <typeparam name="TState">Type of the state that is used to format the error message.</typeparam>
    public class MessageCandidate<TState>
    {
        public MessageCandidate(TState state, LogLevel logLevel, Exception exception, Func<TState, Exception, string> formatter)
        {
            State = state;
            LogLevel = logLevel;
            Exception = exception;
            Formatter = formatter;
        }

        public TState State { get; }

        public LogLevel LogLevel { get; }

        public Exception Exception { get; }

        public Func<TState, Exception, string> Formatter { get; }
    }
}