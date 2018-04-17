namespace Microsoft.Extensions.Logging
{
    using System;
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
        public static ILoggerFactory AddLog4Net(this ILoggerFactory factory, string log4NetConfigFile, Func<object, Exception, string> exceptionFormatter)
        {
            factory.AddProvider(new Log4NetProvider(log4NetConfigFile, exceptionFormatter));
            return factory;
        }

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
        /// <param name="exceptionFormatter">The exception formatter.</param>
        /// <returns>The <see cref="ILoggerFactory"/>.</returns>
        public static ILoggerFactory AddLog4Net(this ILoggerFactory factory, Func<object, Exception, string> exceptionFormatter)
        {
            factory.AddLog4Net(DefaultLog4NetConfigFile, exceptionFormatter);
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
        /// <returns></returns>
        public static ILoggingBuilder AddLog4Net(this ILoggingBuilder builder)
        {
            return builder.AddLog4Net(DefaultLog4NetConfigFile);
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

        /// <summary>
        /// Adds the log4net logging provider.
        /// </summary>
        /// <param name="builder">The logging builder instance.</param>
        /// <param name="log4NetConfigFile">The log4net Config File.</param>
        /// <param name="exceptionFormatter">The exception formatter.</param>
        /// <returns></returns>
        public static ILoggingBuilder AddLog4Net(this ILoggingBuilder builder, string log4NetConfigFile, Func<object, Exception, string> exceptionFormatter)
        {
            builder.Services.AddSingleton<ILoggerProvider>(new Log4NetProvider(log4NetConfigFile, exceptionFormatter));
            return builder; 
        }

        /// <summary>
        /// Adds the log4net logging provider.
        /// </summary>
        /// <param name="builder">The logging builder instance.</param>
        /// <param name="exceptionFormatter">The exception formatter.</param>
        /// <returns></returns>
        public static ILoggingBuilder AddLog4Net(this ILoggingBuilder builder, Func<object, Exception, string> exceptionFormatter)
        {
            builder.Services.AddSingleton<ILoggerProvider>(new Log4NetProvider(DefaultLog4NetConfigFile, exceptionFormatter));
            return builder; 
        }
#endif
    }
}