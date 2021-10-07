using System;
using log4net.Core;
using Microsoft.Extensions.Logging.Log4Net.AspNetCore.Entities;

namespace Microsoft.Extensions.Logging
{
    /// <inheritdoc cref="ILog4NetLoggingEventFactory"/>
    public sealed class Log4NetLoggingEventFactory
        : ILog4NetLoggingEventFactory
    {
        /// <inheritdoc/>
        public LoggingEvent CreateLoggingEvent<TState>(MessageCandidate<TState> messageCandidate, log4net.Core.ILogger logger, Log4NetProviderOptions options)
        {
            Type callerStackBoundaryDeclaringType = typeof(LoggerExtensions);
            string message = messageCandidate.Formatter(messageCandidate.State, messageCandidate.Exception);
            Level logLevel = options.LogLevelTranslator.TranslateLogLevel(messageCandidate.LogLevel, options);

            if (logLevel == null || (string.IsNullOrEmpty(message) && messageCandidate.Exception == null))
                return null;

            var loggingEvent = new LoggingEvent(
                callerStackBoundaryDeclaringType: callerStackBoundaryDeclaringType,
                repository: logger.Repository,
                loggerName: logger.Name,
                level: logLevel,
                message: message,
                exception: messageCandidate.Exception);

            foreach (var scope in messageCandidate.Scopes)
            {
                if(scope.Properties != null)
                {
                    foreach (var scopeProperty in scope.Properties)
                    {
                        loggingEvent.Properties[scopeProperty.Key] = scopeProperty.Value;
                    }
                }
                if (!String.IsNullOrEmpty(scope.Text))
                {
                    loggingEvent.Properties["scope"] = scope.Text;
                }

            }

            return loggingEvent;
        }
    }
}
