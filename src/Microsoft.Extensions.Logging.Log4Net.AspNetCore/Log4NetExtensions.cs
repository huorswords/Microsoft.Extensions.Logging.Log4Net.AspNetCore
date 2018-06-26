namespace Microsoft.Extensions.Logging
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// The log4net extensions class.
    /// </summary>
    public static class Log4NetExtensions
    {
        /// <summary>
        /// The default log4net config file name.
        /// </summary>
        private const string DefaultLog4NetConfigFile = "log4net.config";

        /// <summary>
        /// Adds the log4 net.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="log4NetConfigFile">The log4 net configuration file.</param>
        /// <param name="watch">if set to <c>true</c> [watch].</param>
        /// <returns>The <see cref="ILoggerFactory"/> with added Log4Net provider</returns>
        public static ILoggerFactory AddLog4Net(
            this ILoggerFactory factory,
            string log4NetConfigFile,
            bool watch)
        {
            factory.AddProvider(new Log4NetProvider(log4NetConfigFile, watch));
            return factory;
        }

        /// <summary>
        /// Adds the log4net.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="log4NetConfigFile">The log4 net configuration file.</param>
        /// <param name="configurationSection">The configuration section.</param>
        /// <returns>The <see cref="ILoggerFactory"/> with added Log4Net provider</returns>
        public static ILoggerFactory AddLog4Net(
            this ILoggerFactory factory,
            string log4NetConfigFile,
            IConfigurationSection configurationSection)
        {
            factory.AddProvider(new Log4NetProvider(log4NetConfigFile, configurationSection));
            return factory;
        }

        /// <summary>
        /// Adds the log4net.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="log4NetConfigFile">The log4net Config File.</param>
        /// <returns>The <see cref="ILoggerFactory"/> with added Log4Net provider</returns>
        public static ILoggerFactory AddLog4Net(this ILoggerFactory factory, string log4NetConfigFile) =>
            factory.AddLog4Net(log4NetConfigFile, false);

        /// <summary>
        /// Adds the log4net.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <returns>The <see cref="ILoggerFactory"/> with added Log4Net provider</returns>
        public static ILoggerFactory AddLog4Net(this ILoggerFactory factory)
        {
            factory.AddLog4Net(DefaultLog4NetConfigFile);
            return factory;
        }

#if !NETCOREAPP1_1
        /// <summary>
        /// Adds the log4net logging provider.
        /// </summary>
        /// <param name="builder">The logging builder instance.</param>
        /// <returns>The <see ref="ILoggingBuilder" /> passed as parameter with the new provider registered.</returns>
        public static ILoggingBuilder AddLog4Net(this ILoggingBuilder builder)
        {
            builder.Services.AddSingleton<ILoggerProvider>(new Log4NetProvider(DefaultLog4NetConfigFile));
            return builder; 
        }

        /// <summary>
        /// Adds the log4net logging provider.
        /// </summary>
        /// <param name="builder">The logging builder instance.</param>
        /// <param name="log4NetConfigFile">The log4net Config File.</param>
        /// <returns>The <see ref="ILoggingBuilder" /> passed as parameter with the new provider registered.</returns>
        public static ILoggingBuilder AddLog4Net(this ILoggingBuilder builder, string log4NetConfigFile)
        {
            builder.Services.AddSingleton<ILoggerProvider>(new Log4NetProvider(log4NetConfigFile));
            return builder; 
        }

        /// <summary>
        /// Adds the log4net logging provider.
        /// </summary>
        /// <param name="builder">The logging builder instance.</param>
        /// <param name="log4NetConfigFile">The log4net Config File.</param>
        /// <param name="configurationSection">The configuration section.</param>
        /// <returns>
        /// The <see ref="ILoggingBuilder" /> passed as parameter with the new provider registered.
        /// </returns>
        public static ILoggingBuilder AddLog4Net(this ILoggingBuilder builder, string log4NetConfigFile, IConfigurationSection configurationSection)
        {
            builder.Services.AddSingleton<ILoggerProvider>(new Log4NetProvider(log4NetConfigFile, configurationSection));
            return builder; 
        }

        /// <summary>
        /// Adds the log4net logging provider.
        /// </summary>
        /// <param name="builder">The logging builder instance.</param>
        /// <param name="log4NetConfigFile">The log4net Config File.</param>
        /// <param name="watch">if set to <c>true</c>, the configuration will be reloaded when the xml configuration file changes.</param>
        /// <returns>
        /// The <see ref="ILoggingBuilder" /> passed as parameter with the new provider registered.
        /// </returns>
        public static ILoggingBuilder AddLog4Net(this ILoggingBuilder builder, string log4NetConfigFile, bool watch)
        {
            builder.Services.AddSingleton<ILoggerProvider>(new Log4NetProvider(log4NetConfigFile, watch));
            return builder; 
        }
#endif
    }
}