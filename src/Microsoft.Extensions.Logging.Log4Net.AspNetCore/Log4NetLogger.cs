using log4net;
using log4net.Core;
using Microsoft.Extensions.Logging.Log4Net.AspNetCore.Entities;
using System;

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
            => options.ScopeFactory?.BeginScope(state);

        /// <summary>
        /// Determines whether the logging level is enabled.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <returns>The <see cref="bool"/> value indicating whether the logging level is enabled.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws when <paramref name="logLevel"/> is outside allowed range</exception>
        public bool IsEnabled(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Critical:
                    return this.logger.IsEnabledFor(Level.Fatal);
                case LogLevel.Trace:
                    return this.logger.IsEnabledFor(Level.Trace);
                case LogLevel.Debug:
                    return this.logger.IsEnabledFor(Level.Debug);
                case LogLevel.Error:
                    return this.logger.IsEnabledFor(Level.Error);
                case LogLevel.Information:
                    return this.logger.IsEnabledFor(Level.Info);
                case LogLevel.Warning:
                    return this.logger.IsEnabledFor(Level.Warn);
                case LogLevel.None:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
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

            var message = PrepareMessage(state, logLevel, exception, formatter);
            LogMessage(message);
        }

        private void LogMessage(MessageCandidate candidate)
        {
            if (candidate.IsValid())
            {
                var level = SelectLevel(candidate);
                if (level != null)
                {
                    Log(candidate, level);
                }
            }
        }

        private Level SelectLevel(MessageCandidate candidate)
        {
            Level level = null;
            switch (candidate.LogLevel)
            {
                case LogLevel.Critical:
                    string overrideCriticalLevelWith = options.OverrideCriticalLevelWith;
                    level = !string.IsNullOrEmpty(overrideCriticalLevelWith)
                            && overrideCriticalLevelWith.Equals(LogLevel.Critical.ToString(), StringComparison.OrdinalIgnoreCase)
                                ? Level.Critical
                                : Level.Fatal;
                    break;
                case LogLevel.Debug:
                    level = Level.Debug;
                    break;
                case LogLevel.Error:
                    level = Level.Error;
                    break;
                case LogLevel.Information:
                    level = Level.Info;
                    break;
                case LogLevel.Warning:
                    level = Level.Warn;
                    break;
                case LogLevel.Trace:
                    level = Level.Trace;
                    break;
            }

            return level;
        }

        private void Log(MessageCandidate candidate, Level level)
        {
            var callerStackBoundaryDeclaringType = typeof(LoggerExtensions);
            this.logger.Log(callerStackBoundaryDeclaringType, level, candidate.Message, candidate.Exception);
        }

        private static MessageCandidate PrepareMessage<TState>(TState state, LogLevel logLevel, Exception exception, Func<TState, Exception, string> formatter)
        {
            EnsureValidFormatter(formatter);
            object message = formatter(state, exception);
            return new MessageCandidate(logLevel, message, exception);
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