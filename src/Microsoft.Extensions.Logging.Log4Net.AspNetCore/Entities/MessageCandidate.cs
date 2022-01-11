using System;

namespace Microsoft.Extensions.Logging.Log4Net.AspNetCore.Entities
{
    /// <summary>
    /// Represents a candidate for a log message that should be printed. This candidate will either be accepted or denied by the logger that is trying to print it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is a readonly struct to reduce memory pressure, but because it is quite large (definitly larger than the recommended 16 bytes)
    /// it needs to be passed as a reference (with the in keyword) to make a difference.
    /// </para>
    /// <para>
    /// See <see href="https://devblogs.microsoft.com/premier-developer/the-in-modifier-and-the-readonly-structs-in-c/"/> for more information.
    /// </para>
    /// </remarks>
    /// <typeparam name="TState">Type of the state that is used to format the error message.</typeparam>
    public readonly struct MessageCandidate<TState>
    {
        public MessageCandidate(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            State = state;
            LogLevel = logLevel;
            EventId = eventId;
            Exception = exception;
            Formatter = formatter;
        }

        /// <summary>
        /// The log level the message should be printed with.
        /// </summary>
        public LogLevel LogLevel { get; }

        /// <summary>
        /// The event id of the message.
        /// </summary>
        public EventId EventId { get; }

        /// <summary>
        /// The message state. Can be provided to the formatter to generate the string representation of the error message.
        /// </summary>
        public TState State { get; }

        /// <summary>
        /// Exception that should be printed with the message. Null if the log message has no corrosponding exception.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// The message formatter. Can be called with the state and exception to generate the string representation of the error message.
        /// </summary>
        public Func<TState, Exception, string> Formatter { get; }
    }
}