namespace Microsoft.Extensions.Logging
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;

    using log4net;
    using log4net.Config;
    using log4net.Repository;

    using Microsoft.Extensions.Logging.Log4Net.AspNetCore.Entities;
    using Microsoft.Extensions.Logging.Log4Net.AspNetCore.Extensions;
    using Microsoft.Extensions.Logging.Scope;

    /// <summary>
    /// The log4net provider class.
    /// </summary>
    /// <seealso cref="ILoggerProvider" />
    public partial class Log4NetProvider : ILoggerProvider
    {
        /// <summary>
        /// The loggers collection.
        /// </summary>
        private readonly ConcurrentDictionary<string, Log4NetLogger> loggers = new ConcurrentDictionary<string, Log4NetLogger>();

        /// <summary>
        /// The log4net repository.
        /// </summary>
        private ILoggerRepository loggerRepository;

        /// <summary>
        /// The provider options.
        /// </summary>
        private Log4NetProviderOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetProvider"/> class.
        /// </summary>
        public Log4NetProvider()
            : this(new Log4NetProviderOptions())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetProvider"/> class.
        /// </summary>
        /// <param name="log4NetConfigFileName">The log4NetConfigFile.</param>
        public Log4NetProvider(string log4NetConfigFileName)
            : this(new Log4NetProviderOptions(log4NetConfigFileName))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetProvider"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <exception cref="ArgumentNullException">options</exception>
        /// <exception cref="NotSupportedException">Wach cannot be true when you are overwriting config file values with values from configuration section.</exception>
        public Log4NetProvider(Log4NetProviderOptions options)
        {
            this.SetOptionsIfValid(options);

            Assembly loggingAssembly = GetLoggingReferenceAssembly();

            this.CreateLoggerRepository(loggingAssembly)
                .ConfigureLog4NetLibrary(loggingAssembly);
        }

        /// <summary>
        /// Creates the logger.
        /// </summary>
        /// <returns>An instance of the <see cref="ILogger"/>.</returns>
        public ILogger CreateLogger()
            => this.CreateLogger(this.options.Name);

        /// <summary>
        /// Creates the logger.
        /// </summary>
        /// <param name="categoryName">The category name.</param>
        /// <returns>An instance of the <see cref="ILogger"/>.</returns>
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
                return;
            }

            this.loggers.Clear();
        }

        /// <summary>
        /// Updates configuration nodes overriding values if required.
        /// </summary>
        /// <param name="configXmlDocument">The configuration file XML document.</param>
        /// <param name="overridingNodes">The overriding values available</param>
        /// <returns>An <see cref="XmlDocument"/> within the overriding values replaced.</returns>
        private static XmlDocument UpdateNodesWithOverridingValues(XmlDocument configXmlDocument, IEnumerable<NodeInfo> overridingNodes)
        {
            var additionalConfig = overridingNodes;
            if (additionalConfig != null)
            {
                var configXDoc = configXmlDocument.ToXDocument();
                foreach (var nodeInfo in additionalConfig)
                {
                    var node = configXDoc.XPathSelectElement(nodeInfo.XPath);
                    if (node != null)
                    {
                        if (nodeInfo.NodeContent != null)
                        {
                            node.Value = nodeInfo.NodeContent;
                        }

                        AddOrUpdateAttributes(node, nodeInfo);
                    }
                }

                return configXDoc.ToXmlDocument();
            }

            return configXmlDocument;
        }

        /// <summary>
        /// Adds or updates the attributes specified in the node information.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="nodeInfo">The node information.</param>
        private static void AddOrUpdateAttributes(XElement node, NodeInfo nodeInfo)
        {
            if (nodeInfo?.Attributes != null)
            {
                foreach (var attribute in nodeInfo.Attributes)
                {
                    var nodeAttribute = node.Attributes()
                        .FirstOrDefault(a => a.Name.LocalName.Equals(attribute.Key, StringComparison.OrdinalIgnoreCase));
                    if (nodeAttribute != null)
                    {
                        nodeAttribute.Value = attribute.Value;
                    }
                    else
                    {
                        node.SetAttributeValue(attribute.Key, attribute.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Parses log4net config file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>The <see cref="XmlElement"/> with the log4net XML element.</returns>
        private static XmlDocument ParseLog4NetConfigFile(string filename)
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

                return log4netConfig;
            }
        }

        /// <summary>
        /// Tries to retrieve the assembly from a "Startup" type found in the stack trace.
        /// </summary>
        /// <returns>Null for NetCoreApp 1.1, otherwise, Assembly of Startup type if found in stack trace.</returns>
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

        /// <summary>
        /// Creates the logger implementation.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The <see cref="Log4NetLogger"/> instance.</returns>
        private Log4NetLogger CreateLoggerImplementation(string name)
        {
            var options = new Log4NetProviderOptions
            {
                Name = name,
                LoggerRepository = this.loggerRepository.Name,

            };

            options.ScopeFactory = new Log4NetScopeFactory(new Log4NetScopeRegistry());

            return new Log4NetLogger(options);
        }

        /// <summary>
        /// Gets the current executing assembly considering the target framework.
        /// </summary>
        /// <returns>The assembly to be used as the reference logging assembly.</returns>
        private static Assembly GetLoggingReferenceAssembly()
        {
            Assembly assembly = null;

#if NETCOREAPP1_1
            assembly = Assembly.GetEntryAssembly();
#else
            assembly = Assembly.GetExecutingAssembly();
#endif
            return assembly ?? GetCallingAssemblyFromStartup();
        }

        /// <summary>
        /// Ensures that provided options combinations are valid, and sets the class field if everything is ok.
        /// </summary>
        /// <param name="options">The options to validate.</param>
        /// <exception cref="NotSupportedException">
        /// Throws when the Watch option is set and there are properties to override.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Throws when the options parameter is null.
        /// </exception>
        private void SetOptionsIfValid(Log4NetProviderOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.Watch
                && options.PropertyOverrides.Any())
            {
                throw new NotSupportedException("Wach cannot be true when you are overwriting config file values with values from configuration section.");
            }

            this.options = options;
        }

        /// <summary>
        /// Configures the log4net library using the available configuration data.
        /// </summary>
        /// <param name="assembly">The assembly to be used on the configuration.</param>
        private Log4NetProvider ConfigureLog4NetLibrary(Assembly assembly)
        {
            if (!this.options.ExternalConfigurationSetup)
            {
                string fileNamePath = CreateLog4NetFilePath(assembly);
                if (this.options.Watch)
                {
                    XmlConfigurator.ConfigureAndWatch(
                        this.loggerRepository,
                        new FileInfo(fileNamePath));
                }
                else
                {
                    var configXml = ParseLog4NetConfigFile(fileNamePath);
                    if (this.options.PropertyOverrides != null
                        && this.options.PropertyOverrides.Any())
                    {
                        configXml = UpdateNodesWithOverridingValues(
                            configXml,
                            this.options.PropertyOverrides);
                    }

                    XmlConfigurator.Configure(this.loggerRepository, configXml.DocumentElement);
                }
            }

            return this;
        }

        /// <summary>
        /// Creates the log4net.config file path.
        /// </summary>
        /// <param name="assembly">The assembly to be used when the configuration indicate to use the current assembly.</param>
        /// <returns>The full path to the log4net.config file.</returns>
        private string CreateLog4NetFilePath(Assembly assembly)
        {
            string fileNamePath = this.options.Log4NetConfigFileName;
            if (!Path.IsPathRooted(fileNamePath))
            {
#if NETCOREAPP1_1
                if (!File.Exists(fileNamePath))
                {
                    fileNamePath = Path.Combine(Path.GetDirectoryName(assembly.Location), fileNamePath);
                }
#else
				fileNamePath = Path.Combine(AppContext.BaseDirectory, fileNamePath);
#endif
            }

            return Path.GetFullPath(fileNamePath);
        }

        /// <summary>
        /// Gets or creates the logger repository using the given assembly.
        /// </summary>
        /// <param name="assembly">The assembly to be used to create de repository.</param>
        private Log4NetProvider CreateLoggerRepository(Assembly assembly)
        {
            Type repositoryType = typeof(log4net.Repository.Hierarchy.Hierarchy);

            if (!string.IsNullOrEmpty(this.options.LoggerRepository))
            {
                try
                {
                    this.loggerRepository = LogManager.GetRepository(this.options.LoggerRepository);
                    if (this.options.ExternalConfigurationSetup)
                    {
                        // The logger repository is already configured. We can exit here.
                        return this;
                    }
                }
                catch (log4net.Core.LogException)
                {
                    // The logger repository is not defined outside the extension.
                    this.loggerRepository = null;
                }

                if (this.loggerRepository == null)
                {
                    this.loggerRepository =
                        LogManager.CreateRepository(this.options.LoggerRepository, repositoryType);
                }
            }
            else
            {
                this.loggerRepository =
                    LogManager.CreateRepository(assembly, repositoryType);
            }

            return this;
        }
    }
}