namespace Microsoft.Extensions.Logging
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Xml;
    using System.Xml.XPath;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging.Log4Net.AspNetCore.Extensions;

    using log4net;
    using log4net.Config;

    /// <summary>
    /// The log4net provider class.
    /// </summary>
    /// <seealso cref="ILoggerProvider" />
    /// <remarks>This partial should be REMOVED on next versions.</remarks>
    public partial class Log4NetProvider
    {
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
    }
}