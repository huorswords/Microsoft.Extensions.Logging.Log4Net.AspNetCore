using log4net.Core;
using Microsoft.Extensions.Logging.Log4Net.AspNetCore.Entities;

namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// Represents a factory that creates the log4net <see cref="log4net.Core.LoggingEvent"/> from a <see cref="MessageCandidate{TState}"/>.
    /// </summary>
    public interface ILog4NetLoggingEventFactory
    {
        /// <summary>
        /// Create the <see cref="log4net.Core.LoggingEvent"/>.
        /// </summary>
        /// <typeparam name="TState">Type of the state object that is used to format the log message.</typeparam>
        /// <param name="messageCandidate">The message information that should be logged.</param>
        /// <param name="logger">The logger the event is created for.</param>
        /// <param name="options">The options of the log4net logging provider.</param>
        /// <returns>A <see cref="log4net.Core.LoggingEvent"/> that is ready to be logged with the provided logger or null if the candidate should be dropped.</returns>
        LoggingEvent CreateLoggingEvent<TState>(
            in MessageCandidate<TState> messageCandidate,
            log4net.Core.ILogger logger,
            Log4NetProviderOptions options,
            IExternalScopeProvider scopeProvider);
    }
}