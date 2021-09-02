using log4net.Core;
using System;

namespace Microsoft.Extensions.Logging
{
    /// <inheritdoc cref="ILog4NetLogLevelTranslator"/>
    public sealed class Log4NetLogLevelTranslator : ILog4NetLogLevelTranslator
    {
        /// <inheritdoc/>
        public Level TranslateLogLevel(LogLevel logLevel, Log4NetProviderOptions options)
        {
            Level log4NetLevel = null;
            switch (logLevel)
            {
                case LogLevel.Critical:
                    string overrideCriticalLevelWith = options.OverrideCriticalLevelWith;
                    log4NetLevel = !string.IsNullOrEmpty(overrideCriticalLevelWith)
                            && overrideCriticalLevelWith.Equals(LogLevel.Critical.ToString(), StringComparison.OrdinalIgnoreCase)
                                ? Level.Critical
                                : Level.Fatal;
                    break;
                case LogLevel.Debug:
                    log4NetLevel = Level.Debug;
                    break;
                case LogLevel.Error:
                    log4NetLevel = Level.Error;
                    break;
                case LogLevel.Information:
                    log4NetLevel = Level.Info;
                    break;
                case LogLevel.Warning:
                    log4NetLevel = Level.Warn;
                    break;
                case LogLevel.Trace:
                    log4NetLevel = Level.Trace;
                    break;
            }

            return log4NetLevel;
        }
    }
}
