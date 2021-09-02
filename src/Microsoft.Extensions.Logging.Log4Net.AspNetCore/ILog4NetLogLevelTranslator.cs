namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// Represents a log level translator between the different logging systems.
    /// </summary>
    public interface ILog4NetLogLevelTranslator
    {
        /// <summary>
        /// Translates a <see cref="LogLevel"/> to a log4net <see cref="log4net.Core.Level"/> based on the provided options.
        /// </summary>
        /// <param name="logLevel">The log level to translate.</param>
        /// <param name="options">The log4net provider options influencing the translation.</param>
        /// <returns>The corresponding log level for log4net.</returns>
        log4net.Core.Level TranslateLogLevel(LogLevel logLevel, Log4NetProviderOptions options);
    }
}
