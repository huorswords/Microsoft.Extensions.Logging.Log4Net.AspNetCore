﻿using Microsoft.Extensions.Logging.Log4Net.AspNetCore.Entities;
using Microsoft.Extensions.Logging.Scope;
using System.Collections.Generic;

namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// The log4Net provider options.
    /// </summary>
    public sealed class Log4NetProviderOptions
    {
        /// <summary>
        /// The default log4 net file name
        /// </summary>
        private const string DefaultLog4NetFileName = "log4net.config";

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetProviderOptions"/> class.
        /// </summary>
        public Log4NetProviderOptions()
            : this(DefaultLog4NetFileName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetProviderOptions"/> class.
        /// </summary>
        /// <param name="log4NetConfigFileName">Name of the log4 net configuration file.</param>
        public Log4NetProviderOptions(string log4NetConfigFileName)
            : this(log4NetConfigFileName, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetProviderOptions"/> class.
        /// </summary>
        /// <param name="log4NetConfigFileName">Name of the log4net configuration file.</param>
        public Log4NetProviderOptions(string log4NetConfigFileName, bool watch)
        {
            Log4NetConfigFileName = log4NetConfigFileName;
            Watch = watch;

            OverrideCriticalLevelWith = string.Empty;
            Name = string.Empty;
            PropertyOverrides = new List<NodeInfo>();
            ExternalConfigurationSetup = false;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the log file.
        /// </summary>
        public string Log4NetConfigFileName { get; set; }

        /// <summary>
        /// Gets or sets the logger repository.
        /// </summary>
        public string LoggerRepository { get; set; }

        /// <summary>
        /// Gets or sets the level value that should be used to override default's critical level.
        /// </summary>
        public string OverrideCriticalLevelWith { get; set; }

        /// <summary>
        /// Gets or sets the property overrides.
        /// </summary>
        public List<NodeInfo> PropertyOverrides { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Log4NetProviderOptions"/> is watch.
        /// </summary>
        public bool Watch { get; set; }

        /// <summary>
        /// Gets or sets the scope factory.
        /// </summary>
        public ILog4NetScopeFactory ScopeFactory { get; set; }

        /// <summary>
        /// Let user setup log4net externally
        /// </summary>
        public bool ExternalConfigurationSetup { get; set; }

        /// <summary>
        /// Let user setup log4net from web.config / app.config.
        /// </summary>
        public bool UseWebOrAppConfig { get; set; }
    }
}