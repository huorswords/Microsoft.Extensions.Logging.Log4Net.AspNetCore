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

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging.Log4Net.AspNetCore.Entities;
    using Microsoft.Extensions.Logging.Log4Net.AspNetCore.Extensions;
    using Microsoft.Extensions.Logging.Scope;

    /// <summary>
    /// The log4net provider class.
    /// </summary>
    /// <seealso cref="ILoggerProvider" />
    public class Log4NetProvider : ILoggerProvider
	{
		/// <summary>
		/// The log4net repository.
		/// </summary>
		private readonly ILoggerRepository loggerRepository;

		/// <summary>
		/// The loggers collection.
		/// </summary>
		private readonly ConcurrentDictionary<string, Log4NetLogger> loggers = new ConcurrentDictionary<string, Log4NetLogger>();

		/// <summary>
		/// The provider options.
		/// </summary>
		private readonly Log4NetProviderOptions options;

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
			this.options = options ?? throw new ArgumentNullException(nameof(options));

			if (options.Watch
				&& options.PropertyOverrides.Any())
			{
				throw new NotSupportedException("Wach cannot be true when you are overwriting config file values with values from configuration section.");
			}

			Assembly assembly = null;
			if (string.IsNullOrEmpty(this.options.LoggerRepository))
			{

#if NETCOREAPP1_1
				assembly = Assembly.GetEntryAssembly();
#else
				assembly = Assembly.GetExecutingAssembly();
#endif
				this.loggerRepository = LogManager.CreateRepository(
					assembly ?? GetCallingAssemblyFromStartup(),
					typeof(log4net.Repository.Hierarchy.Hierarchy));
			}
			else
			{
				this.loggerRepository = LogManager.GetRepository(options.LoggerRepository) ?? LogManager.CreateRepository(
						options.LoggerRepository,
						typeof(log4net.Repository.Hierarchy.Hierarchy));
			}

			if (options.ExternalConfigurationSetup)
			{
				return;
			}
			
			string fileNamePath = options.Log4NetConfigFileName;
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

			fileNamePath = Path.GetFullPath(fileNamePath);
			if (options.Watch)
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

		/// <summary>
		/// Initializes a new instance of the <see cref="Log4NetProvider"/> class.
		/// </summary>
		/// <param name="log4NetConfigFile">The log4 net configuration file.</param>
		/// <param name="configurationSection">The configuration section.</param>
		[Obsolete("Use Log4NetProvider(Log4NetProviderOptions) instead.")]
		public Log4NetProvider(string log4NetConfigFile, IConfigurationSection configurationSection)
			: this(log4NetConfigFile, false, configurationSection)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Log4NetProvider"/> class.
		/// </summary>
		/// <param name="log4NetConfigFile">The log4 net configuration file.</param>
		/// <param name="watch">if set to <c>true</c> [watch].</param>
		[Obsolete("Use Log4NetProvider(Log4NetProviderOptions) instead.")]
		public Log4NetProvider(string log4NetConfigFile, bool watch)
			: this(log4NetConfigFile, watch, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Log4NetProvider" /> class.
		/// </summary>
		/// <param name="log4NetConfigFile">The log4 net configuration file.</param>
		/// <param name="watch">if set to <c>true</c> [watch].</param>
		/// <param name="configurationSection">The configuration section.</param>
		/// <exception cref="NotSupportedException">Watch cannot be true if you are overwriting config file values with values from configuration section.</exception>
		private Log4NetProvider(string log4NetConfigFile, bool watch, IConfigurationSection configurationSection)
		{
			if (watch && configurationSection != null)
			{
				throw new NotSupportedException("Wach cannot be true if you are overwriting config file values with values from configuration section.");
			}

#if NETCOREAPP1_1
			Assembly assembly = Assembly.GetEntryAssembly();
#else
			Assembly assembly = Assembly.GetExecutingAssembly();
#endif
			this.loggerRepository = LogManager.CreateRepository(
				assembly ?? GetCallingAssemblyFromStartup(),
				typeof(log4net.Repository.Hierarchy.Hierarchy));

			if (watch)
			{
				XmlConfigurator.ConfigureAndWatch(this.loggerRepository, new FileInfo(Path.GetFullPath(log4NetConfigFile)));
			}
			else
			{
				var configXml = ParseLog4NetConfigFile(log4NetConfigFile);
				if (configurationSection != null)
				{
					configXml = UpdateNodesWithAdditionalConfiguration(configXml, configurationSection);
				}

				XmlConfigurator.Configure(this.loggerRepository, configXml.DocumentElement);
			}
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
		/// Rewrites the information of the node specified by xpath expression.
		/// </summary>
		/// <param name="configXml">The log4net config in xml.</param>
		/// <param name="configurationSection">The configuration section.</param>
		/// <returns>The xml configuration with overwritten  nodes if any</returns>
		private static XmlDocument UpdateNodesWithAdditionalConfiguration(XmlDocument configXml, IConfigurationSection configurationSection)
		{
			var additionalConfig = configurationSection.ToNodesInfo();
			if (additionalConfig != null)
			{
				var configXDoc = configXml.ToXDocument();
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

			return configXml;
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
    }
}