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
        {
            SetupProvider(log4NetConfigFile, new Log4NetParams());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetProvider"/> class.
        /// </summary>
        /// <param name="log4NetConfigFile">The log4NetConfigFile.</param>
        /// <param name="log4NetParams">The log4net Config Parameters.</param>
        public Log4NetProvider(string log4NetConfigFile, Log4NetParams log4NetParams)
        {
            SetupProvider(log4NetConfigFile, log4NetParams);
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

        private void SetupProvider(string log4NetConfigFile, Log4NetParams log4NetParams)
        {
            loggerRepository = LogManager.CreateRepository(Assembly.GetEntryAssembly() ?? GetCallingAssemblyFromStartup(),
                typeof(log4net.Repository.Hierarchy.Hierarchy));

            var log4NetXml = Parselog4NetConfigFile(log4NetConfigFile);
            if (log4NetParams != null)
            {
                log4NetXml = SetParams(log4NetXml, log4NetParams);

                if (log4NetParams.Watch)
                {
                    var log4NetConfigWatchFile = "watch-" + log4NetConfigFile;
                    var xmlDoc = new XmlDocument();

                    //necessary for crossing XmlDocument contexts
                    var importNode = xmlDoc.ImportNode(log4NetXml, true);

                    // Write down the XML declaration
                    var xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                    xmlDoc.InsertBefore(xmlDeclaration, xmlDoc.DocumentElement);
                    xmlDoc.AppendChild(importNode);

                    // Save to the XML file
                    var stream = File.Create(log4NetConfigWatchFile);
                    xmlDoc.Save(stream);
                    stream.Dispose();

                    XmlConfigurator.ConfigureAndWatch(loggerRepository, new FileInfo(log4NetConfigWatchFile));
                    return;
                }
            }
            XmlConfigurator.Configure(loggerRepository, log4NetXml);
        }

        private XmlElement SetParams(XmlElement log4NetXml, Log4NetParams log4NetParams)
        {
            if (log4NetParams.FileName != null)
            {
                var fileNodes = log4NetXml.GetElementsByTagName("file");
                var fileNode = fileNodes.Item(0);
                fileNode.Attributes["value"].Value = log4NetParams.FileName;
            }

            return log4NetXml;
        }
    }
}