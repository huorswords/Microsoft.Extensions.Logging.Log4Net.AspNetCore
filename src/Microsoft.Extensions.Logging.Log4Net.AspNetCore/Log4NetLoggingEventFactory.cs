using log4net.Core;
using Microsoft.Extensions.Logging.Log4Net.AspNetCore.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Extensions.Logging
{
    /// <inheritdoc cref="ILog4NetLoggingEventFactory"/>
    public class Log4NetLoggingEventFactory
        : ILog4NetLoggingEventFactory
    {
        /// <summary>
        /// The default property name for scopes that don't provide their own property name by implementing
        /// an <see cref="IEnumerable{T}"/> where T is <see cref="KeyValuePair{TKey,TValue}"/> and where TKey
        /// is <see cref="string"/>.
        /// </summary>
        protected const string DefaultScopeProperty = "scope";

        /// <inheritdoc/>
        public LoggingEvent CreateLoggingEvent<TState>(
            in MessageCandidate<TState> messageCandidate,
            log4net.Core.ILogger logger,
            Log4NetProviderOptions options,
            IExternalScopeProvider scopeProvider)
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

            EnrichWithScopes(loggingEvent, scopeProvider);

            return loggingEvent;
        }


        /// <summary>
        /// Gets the scopes from the external scope provider and converts them to the properties on the logging event.
        /// This function will honor the convention that logging scopes can provide their own property name, by implementing
        /// an <see cref="IEnumerable{T}"/> where T is <see cref="KeyValuePair{TKey,TValue}"/> and where TKey is
        /// <see cref="string"/>.
        /// </summary>
        /// <remarks>
        /// The default implementation will call Convert.ToString(scope, CultureInfo.InvariantCulture) on all scope objects.
        /// If you want to do this conversion inside the Log4Net Pipeline, e. g. with a custom layout, you can override this
        /// method and change the behaviour.
        /// </remarks>
        /// <param name="loggingEvent">The <see cref="LoggingEvent"/> the scope information will be added to.</param>
        /// <param name="scopeProvider">The external provider for the current logging scope.</param>
        protected virtual void EnrichWithScopes(LoggingEvent loggingEvent, IExternalScopeProvider scopeProvider)
        {
            scopeProvider.ForEachScope((scope, @event) =>
            {
                // This function will add the scopes in the legacy way they were added before the IExternalScopeProvider was introduced,
                // to maintain backwards compatibility.
                // This pretty much means that we are emulating a LogicalThreadContextStack, which is a stack, that allows pushing
                // strings on to it, which will be concatenated with space as a separator.
                // See: https://github.com/apache/logging-log4net/blob/47aaf46d5f031ea29d781bac4617bd1bb9446215/src/log4net/Util/LogicalThreadContextStack.cs#L343

                // Because string implements IEnumerable we first need to check for string.
                if (scope is string)
                {
                    string previousValue = @event.Properties[DefaultScopeProperty] as string;

                    @event.Properties[DefaultScopeProperty] = JoinOldAndNewValue(previousValue, scope.ToString());
                    return;
                }

                if (scope is IEnumerable col)
                {
                    foreach (var item in col)
                    {
                        if (item is KeyValuePair<string, string>)
                        {
                            var keyValuePair = (KeyValuePair<string, string>)item;
                            string previousValue = @event.Properties[keyValuePair.Key] as string;
                            @event.Properties[keyValuePair.Key] = JoinOldAndNewValue(previousValue, keyValuePair.Value);
                            continue;
                        }

                        if (item is KeyValuePair<string, object>)
                        {
                            var keyValuePair = (KeyValuePair<string, object>)item;
                            string previousValue = @event.Properties[keyValuePair.Key] as string;

                            // The current culture should not influence how integers/floats/... are displayed in logging,
                            // so we are using Convert.ToString which will convert IConvertible and IFormattable with
                            // the specified IFormatProvider.
                            string additionalValue = Convert.ToString(keyValuePair.Value, CultureInfo.InvariantCulture);
                            @event.Properties[keyValuePair.Key] = JoinOldAndNewValue(previousValue, additionalValue);
                            continue;
                        }
                    }
                    return;
                }

                if (scope is object)
                {
                    string previousValue = @event.Properties[DefaultScopeProperty] as string;
                    string additionalValue = Convert.ToString(scope, CultureInfo.InvariantCulture);
                    @event.Properties[DefaultScopeProperty] = JoinOldAndNewValue(previousValue, additionalValue);
                    return;
                }

            }, loggingEvent);
        }

        private static string JoinOldAndNewValue(string previousValue, string newValue)
        {
            if (string.IsNullOrEmpty(previousValue))
            {
                return newValue;
            }

            return previousValue + " " + newValue;
        }
    }
}
