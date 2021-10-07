﻿using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.Logging.Log4Net.AspNetCore.Entities
{
    /// <summary>
    /// Represents a candidate for a log message that should be printed. This candidate will either be accepted or denied by the logger that is trying to print it.
    /// </summary>
    /// <typeparam name="TState">Type of the state that is used to format the error message.</typeparam>
    public class MessageCandidate<TState>
    {
        public MessageCandidate(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter, IList<ScopeInfo> scopes)
        {
            State = state;
            LogLevel = logLevel;
            EventId = eventId;
            Exception = exception;
            Formatter = formatter;
            Scopes = scopes;
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

        public IList<ScopeInfo> Scopes { get;  } 
    }
}