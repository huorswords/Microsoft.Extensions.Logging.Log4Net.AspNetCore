namespace Microsoft.Extensions.Logging
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Reflection;
    using System.Xml;

    using log4net;
    using log4net.Config;
    using log4net.Repository;

    /// <summary>
    /// Log4Net provider extensions.
    /// </summary>
    public static class Log4NetProviderExtensions
    {
        /// <summary>
        /// Creates a logger with the name of the given <see cref="TName"/> type.
        /// </summary>
        /// <typeparam name="TName">The type of the class to be used as name of the logger.</typeparam>
        /// <param name="self">An ILoggerProvider instance.</param>
        /// <returns>An instance of the <see cref="ILogger"/>.</returns>
        public static ILogger CreateLogger<TName>(this ILoggerProvider self) where TName : class
        {
            if (self == null)
            {
                throw new ArgumentNullException(nameof(self));
            }
            
            if (!self.GetType().IsAssignableFrom(typeof(Log4NetProvider)))
            {
                throw new ArgumentOutOfRangeException(nameof(self), "The ILoggerProvider should be of type Log4NetProvider.");
            }

            return self.CreateLogger(typeof(TName).FullName);
        }
    }

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
        /// The loggers collection.
        /// </summary>
        private readonly ConcurrentDictionary<string, Log4NetLogger> loggers = new ConcurrentDictionary<string, Log4NetLogger>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetProvider"/> class.
        /// </summary>
        /// <param name="log4NetConfigFile">The log4NetConfigFile.</param>
        public Log4NetProvider(string log4NetConfigFile)
            : this(log4NetConfigFile, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetProvider"/> class.
        /// </summary>
        /// <param name="log4NetConfigFile">The log4 net configuration file.</param>
        /// <param name="watch">if set to <c>true</c> [watch].</param>
        public Log4NetProvider(string log4NetConfigFile, bool watch)
        {
            loggerRepository = LogManager.CreateRepository(Assembly.GetEntryAssembly() ?? GetCallingAssemblyFromStartup(),
                                                           typeof(log4net.Repository.Hierarchy.Hierarchy));
            if (watch)
            {
                XmlConfigurator.ConfigureAndWatch(loggerRepository, new FileInfo(Path.GetFullPath(log4NetConfigFile)));
            }
            else
            {
                XmlConfigurator.Configure(loggerRepository, Parselog4NetConfigFile(log4NetConfigFile));
            }
        }

        /// <summary>
        /// Creates the logger.
        /// </summary>
        /// <param name="categoryName">The category name.</param>
        /// <returns>The <see cref="ILogger"/> instance.</returns>
        public ILogger CreateLogger(string categoryName)
            => this.loggers.GetOrAdd(categoryName, this.CreateLoggerImplementation);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

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
            => new Log4NetLogger(loggerRepository.Name, name);

        /// <summary>
        /// Tries to retrieve the assembly from a "Startup" type found in the stacktrace.
        /// </summary>
        /// <returns>Null for NetCoreApp 1.1 otherwise Assembly of Startup type if found in stacktrace.</returns>
        private static Assembly GetCallingAssemblyFromStartup()
        {
#if NETCOREAPP1_1
            return null;
#else
            var stackTrace = new System.Diagnostics.StackTrace(2);

            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                var frame = stackTrace.GetFrame(i);
                var type = frame.GetMethod()?.DeclaringType;

                if (string.Equals(type?.Name, "Startup", StringComparison.OrdinalIgnoreCase))
                {
                    return type.Assembly;
                }
            }

            return null;
#endif
        }
    }
}