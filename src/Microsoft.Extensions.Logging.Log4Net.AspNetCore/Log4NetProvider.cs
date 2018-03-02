using System.Reflection;
using log4net;
using log4net.Config;
using log4net.Repository;

namespace Microsoft.Extensions.Logging
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Text;
    using System.Xml;

    /// <summary>
    /// The log4net provider class.
    /// </summary>
    public class Log4NetProvider : ILoggerProvider
    {
        /// <summary>
        /// The log4net repository.
        /// </summary>
        private ILoggerRepository loggerRepository;

        /// <summary>
        /// The exception formatter Func.
        /// </summary>
        private Func<object, Exception, string> exceptionFormatter;

        /// <summary>
        /// The loggers collection.
        /// </summary>
        private readonly ConcurrentDictionary<string, Log4NetLogger> loggers = new ConcurrentDictionary<string, Log4NetLogger>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetProvider"/> class.
        /// </summary>
        /// <param name="log4NetConfigFile">The log4NetConfigFile.</param>
        public Log4NetProvider(string log4NetConfigFile, Func<object, Exception, string> exceptionFormatter)
        {
            this.exceptionFormatter = exceptionFormatter ?? FormatExceptionByDefault;
            loggerRepository = LogManager.CreateRepository(Assembly.GetEntryAssembly(),
                                                           typeof(log4net.Repository.Hierarchy.Hierarchy));
            XmlConfigurator.Configure(loggerRepository, Parselog4NetConfigFile(log4NetConfigFile));
        }

        /// <summary>
        /// Creates the logger.
        /// </summary>
        /// <param name="categoryName">The category name.</param>
        /// <returns>The <see cref="ILogger"/> instance.</returns>
        public ILogger CreateLogger(string categoryName)
        {
            return this.loggers.GetOrAdd(categoryName, this.CreateLoggerImplementation);
        }

        /// <summary>
        /// Disposes the provider.
        /// </summary>
        public void Dispose()
        {
            this.loggers.Clear();
        }

        /// <summary>
        /// Parses log4net config file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>The <see cref="XmlElement"/> with the log4net XML element.</returns>
        private static XmlElement Parselog4NetConfigFile(string filename)
        {
            using (FileStream fp = File.OpenRead(filename))
            {
                var settings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Prohibit
                };

                var log4netConfig = new XmlDocument();
                using (var reader = XmlReader.Create(fp, settings))
                {
                    log4netConfig.Load(reader);
                }

                return log4netConfig["log4net"];
            }
        }

        /// <summary>
        /// Creates the logger implementation.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The <see cref="Log4NetLogger"/> instance.</returns>
        private Log4NetLogger CreateLoggerImplementation(string name)
        {
            return new Log4NetLogger(loggerRepository.Name, name)
                       .UsingCustomExceptionFormatter(exceptionFormatter);
        }

        /// <summary>
        /// Formats an exception by default.
        /// </summary>
        /// <typeparam name="TState">The type of the state.</typeparam>
        /// <param name="state">The state of the logged object.</param>
        /// <param name="exception">The exception to be logged.</param>
        /// <returns>The text with the formatted message.</returns>
        private static string FormatExceptionByDefault<TState>(TState state, Exception exception)
        {
            var builder = new StringBuilder();
            builder.Append(state.ToString());
            builder.Append(" - ");
            if (exception != null)
            {
                builder.Append(exception.ToString());
            }

            return builder.ToString();
        }
    }
}