namespace Microsoft.Extensions.Logging
{
    using System;
    using log4net;

    /// <summary>
    /// The log4net logger class.
    /// </summary>
    public class Log4NetLogger : ILogger
    {
        /// <summary>
        /// The log.
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// The formatter when logging an exception.
        /// </summary>
        private Func<object, Exception, string> exceptionDetailsFormatter;

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetLogger"/> class.
        /// </summary>
        /// <param name="loggerRepository">The repository name.</param>
        /// <param name="name">The logger's name.</param>
        public Log4NetLogger(string loggerRepository, string name)
        {
            this.log = LogManager.GetLogger(loggerRepository, name);
        }
        
        /// <summary>
        /// Begins a logical operation scope.
        /// </summary>
        /// <typeparam name="TState">The type of the state.</typeparam>
        /// <param name="state">The identifier for the scope.</param>
        /// <returns>
        /// An IDisposable that ends the logical operation scope on dispose.
        /// </returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        /// <summary>
        /// Determines whether the logging level is enabled.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <returns>The <see cref="bool"/> value indicating whether the logging level is enabled.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public bool IsEnabled(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Critical:
                    return log.IsFatalEnabled;
                case LogLevel.Debug:
                case LogLevel.Trace:
                    return log.IsDebugEnabled;
                case LogLevel.Error:
                    return log.IsErrorEnabled;
                case LogLevel.Information:
                    return log.IsInfoEnabled;
                case LogLevel.Warning:
                    return log.IsWarnEnabled;
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
            
            if (null == formatter)
            {
                throw new ArgumentNullException(nameof(formatter));
            }
            
            string message = null;
            if (null != formatter)
            {
                message = formatter(state, exception);
            }

            if (null != exception && null != this.exceptionDetailsFormatter)
            {
                message = this.exceptionDetailsFormatter(message, exception);
            }

            if (!string.IsNullOrEmpty(message)
                || exception != null)
            {
                switch (logLevel)
                {
                    case LogLevel.Critical:
                        log.Fatal(message);
                        break;
                    case LogLevel.Debug:
                    case LogLevel.Trace:
                        log.Debug(message);
                        break;
                    case LogLevel.Error:
                        log.Error(message);
                        break;
                    case LogLevel.Information:
                        log.Info(message);
                        break;
                    case LogLevel.Warning:
                        log.Warn(message);
                        break;
                    default:
                        log.Warn($"Encountered unknown log level {logLevel}, writing out as Info.");
                        log.Info(message, exception);
                        break;
                }
            }
        }

        /// <summary>
        /// Defines custom formatter for logging exceptions.
        /// </summary>
        /// <param name="formatter">The formatting function to be used when formatting exceptions.</param>
        /// <returns>The logger itself for fluent use.</returns>
        public Log4NetLogger UsingCustomExceptionFormatter(Func<object, Exception, string> formatter)
        {
            this.exceptionDetailsFormatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            return this;
        }
    }
}