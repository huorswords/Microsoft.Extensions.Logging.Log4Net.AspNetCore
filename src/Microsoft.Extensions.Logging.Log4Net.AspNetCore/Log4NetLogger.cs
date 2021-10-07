﻿using System;
using System.Collections.Generic;
using log4net;
using log4net.Core;
using Microsoft.Extensions.Logging.Log4Net.AspNetCore.Entities;

namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// The log4net logger class.
    /// </summary>
    public class Log4NetLogger : ILogger
    {
        /// <summary>
        /// The log.
        /// </summary>
        private readonly log4net.Core.ILogger logger; 

        /// <summary>
        /// The provider options.
        /// </summary>
        private readonly Log4NetProviderOptions options;

        public IExternalScopeProvider ScopeProvider { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetLogger"/> class.
        /// </summary>
        /// <param name="options">The log4net provider options.</param>
        public Log4NetLogger(Log4NetProviderOptions options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.logger = LogManager.GetLogger(options.LoggerRepository, options.Name).Logger;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name
            => this.logger.Name;

        /// <summary>
        /// A get-only property for accessing the <see cref="Log4NetProviderOptions"/>
        /// within the instance.
        /// </summary>
        internal Log4NetProviderOptions Options => this.options;

        /// <summary>
        /// Begins a logical operation scope.
        /// </summary>
        /// <typeparam name="TState">The type of the state.</typeparam>
        /// <param name="state">The identifier for the scope.</param>
        /// <returns>
        /// An IDisposable that ends the logical operation scope on dispose.
        /// </returns>
        public IDisposable BeginScope<TState>(TState state)
            => ScopeProvider.Push(state);

        /// <summary>
        /// Determines whether the logging level is enabled.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <returns>The <see cref="bool"/> value indicating whether the logging level is enabled.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws when <paramref name="logLevel"/> is outside allowed range.</exception>
        public bool IsEnabled(LogLevel logLevel)
        {
            Level translatedLogLevel = this.options.LogLevelTranslator.TranslateLogLevel(logLevel, Options);
            if (translatedLogLevel != null)
            {
                return this.logger.IsEnabledFor(translatedLogLevel);
            }

            if (logLevel == LogLevel.None)
            {
                return false;
            }

            throw new ArgumentOutOfRangeException(nameof(logLevel));
        }

        /// <summary>
        /// Logs an exception into the log.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="eventId">The event Id.</param>
        /// <param name="state">The state.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="formatter">The formatter.</param>
        /// <typeparam name="TState">The type of the state.</typeparam>
        /// <exception cref="ArgumentNullException">Throws when the <paramref name="formatter"/> is null.</exception>
        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (!this.IsEnabled(logLevel))
            {
                return;
            }

            var message = PrepareMessage(logLevel, eventId, state, exception, formatter, ScopeProvider);
            LogMessage(message);
        }

        private void LogMessage<TState>(MessageCandidate<TState> candidate)
        {
            LoggingEvent loggingEvent = options.LoggingEventFactory.CreateLoggingEvent(candidate, logger, options);
            
            if (loggingEvent == null)
                return;

            this.logger.Log(loggingEvent);
        }

        private static MessageCandidate<TState> PrepareMessage<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter, IExternalScopeProvider externalScopeProvider)
        {
            EnsureValidFormatter(formatter);

            var scopes = new List<ScopeInfo>(10);

            // gather info about scope(s), if any
            if (externalScopeProvider != null)
            {
                externalScopeProvider.ForEachScope((value, loggingProps) =>
                {
                    var scope = new ScopeInfo();
                    scopes.Add(scope);

                    if (value is string)
                    {
                        scope.Text = value.ToString();
                    }
                    else if (value is IEnumerable<KeyValuePair<string, object>> props)
                    {
                        if (scope.Properties == null)
                            scope.Properties = new Dictionary<string, object>();

                        foreach (var pair in props)
                        {
                            scope.Properties[pair.Key] = pair.Value;
                        }
                    }
                },
                state);

            }

            return new MessageCandidate<TState>(logLevel, eventId, state, exception, formatter, scopes);
        }

        private static void EnsureValidFormatter<TState>(Func<TState, Exception, string> formatter)
        {
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }
        }
    }
}