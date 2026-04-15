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
        private const string EventIdProperty = "eventId";

        /// <summary>
        /// The default property name for scopes that don't provide their own property name by implementing
        /// an <see cref="IEnumerable{T}"/> where T is <see cref="KeyValuePair{TKey,TValue}"/> and where TKey
        /// is <see cref="string"/>.
        /// </summary>
        protected const string DefaultScopeProperty = "scope";

        /// <summary>
        /// The property name for message format as used in Microsoft.Extensions.Logging.FormattedLogValues.
        /// </summary>
        protected const string OriginalFormatProperty = "{OriginalFormat}";

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
            // in case scope and formatted message contain arguments with the same names, formatted message should win
            EnrichProperties(loggingEvent, in messageCandidate);

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
                        if (item is KeyValuePair<string, string> kvpString)
                        {
                            string previousValue = @event.Properties[kvpString.Key] as string;
                            @event.Properties[kvpString.Key] = JoinOldAndNewValue(previousValue, kvpString.Value);
                            continue;
                        }

                        if (item is KeyValuePair<string, object> kvpObject)
                        {
                            string previousValue = @event.Properties[kvpObject.Key] as string;

                            // The current culture should not influence how integers/floats/... are displayed in logging,
                            // so we are using Convert.ToString which will convert IConvertible and IFormattable with
                            // the specified IFormatProvider.
                            string additionalValue = ConvertValue(kvpObject.Value);
                            @event.Properties[kvpObject.Key] = JoinOldAndNewValue(previousValue, additionalValue);
                            continue;
                        }
                    }
                    return;
                }

                if (FromValueTuple<string>())
                    return;
                if (FromValueTuple<int>())
                    return;
                if (FromValueTuple<long>())
                    return;
                if (FromValueTuple<short>())
                    return;
                if (FromValueTuple<decimal>())
                    return;
                if (FromValueTuple<double>())
                    return;
                if (FromValueTuple<float>())
                    return;
                if (FromValueTuple<uint>())
                    return;
                if (FromValueTuple<ulong>())
                    return;
                if (FromValueTuple<ushort>())
                    return;
                if (FromValueTuple<byte>())
                    return;
                if (FromValueTuple<sbyte>())
                    return;
                if (FromValueTuple<object>())
                    return;

                if (scope is object)
                {
                    string previousValue = @event.Properties[DefaultScopeProperty] as string;
                    string additionalValue = ConvertValue(scope);
                    @event.Properties[DefaultScopeProperty] = JoinOldAndNewValue(previousValue, additionalValue);
                    return;
                }

                bool FromValueTuple<T>()
                {
                    if (scope is ValueTuple<string, T> valueTuple)
                    {
                        string previousValue = @event.Properties[valueTuple.Item1] as string;
                        string additionalValue = ConvertValue(valueTuple.Item2);
                        @event.Properties[valueTuple.Item1] = JoinOldAndNewValue(previousValue, additionalValue);
                        return true;
                    }
                    return false;
                }

            }, loggingEvent);
        }

        /// <summary>
        /// Enriches logging event with additional properties.
        /// </summary>
        /// <remarks>
        /// The default implementation will add the event id, the original format string as the "MessageTemplate" property,
        /// and argument values from <paramref name="messageCandidate"/>.<see cref="MessageCandidate<TState>.State"/>, if any.
        /// Argument values are added as strings using <see cref="ConvertValue{T}(T)"/>, which uses
        /// <see cref="Convert.ToString(object, IFormatProvider)"/> with <see cref="CultureInfo.InvariantCulture"/>.
        /// If you want to do this conversion inside the Log4Net pipeline, for example with a custom layout, you can
        /// override this method and change the behavior.
        /// </remarks>
        /// <typeparam name="TState">Type of the state that is used to format the error message.</typeparam>
        /// <param name="loggingEvent">The <see cref="LoggingEvent"/> properties will be added to.</param>
        /// <param name="messageCandidate">Log message candidate.</param>
        protected virtual void EnrichProperties<TState>(LoggingEvent loggingEvent, in MessageCandidate<TState> messageCandidate)
        {
            loggingEvent.Properties[EventIdProperty] = messageCandidate.EventId;

            // State is always passed as Microsoft.Extensions.Logging.FormattedLogValues which is internal
            // but implements IReadOnlyCollection<KeyValuePair<string, object>>
            if (messageCandidate.State is IReadOnlyCollection<KeyValuePair<string, object>> stateProperties)
            {
                foreach (var kvp in stateProperties)
                {
                    var key = kvp.Key;
                    if (kvp.Key == OriginalFormatProperty && kvp.Value is string)
                    {
                        // change property name to match Serilog
                        key = "MessageTemplate";
                    }
                    loggingEvent.Properties[key] = ConvertValue(kvp.Value);
                }
            }
        }

        private static string ConvertValue<T>(T value)
        {
            var convertedValue = Convert.ToString(value, CultureInfo.InvariantCulture);
            return convertedValue;
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
