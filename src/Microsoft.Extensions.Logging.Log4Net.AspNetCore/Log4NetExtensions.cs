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
		/// Adds the log4net.
		/// </summary>
		/// <param name="factory">The factory.</param>
		/// <param name="log4NetConfigFile">The log4net Config File.</param>
		/// <returns>The <see cref="ILoggerFactory"/>.</returns>
		public static ILoggerFactory AddLog4Net(this ILoggerFactory factory, string log4NetConfigFile)
        {
            factory.AddProvider(new Log4NetProvider(log4NetConfigFile));
            return factory;
        }

        /// <summary>
        /// Adds the log4net.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <returns>The <see cref="ILoggerFactory"/>.</returns>
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
        /// <param name="exceptionFormatter">The exception formatter.</param>
        /// <returns></returns>
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
        /// <returns></returns>
        public static ILoggingBuilder AddLog4Net(this ILoggingBuilder builder, string log4NetConfigFile)
        {
            builder.Services.AddSingleton<ILoggerProvider>(new Log4NetProvider(log4NetConfigFile));
            return builder; 
        }
#endif
    }
}