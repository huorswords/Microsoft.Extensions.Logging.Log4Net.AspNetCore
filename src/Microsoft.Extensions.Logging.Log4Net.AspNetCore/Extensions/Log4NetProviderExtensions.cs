﻿using System;
#if NETCOREAPP1_1
using System.Reflection;
#endif

namespace Microsoft.Extensions.Logging.Extensions
{
    /// <summary>
    /// Log4Net provider extensions.
    /// </summary>
    public static class Log4NetProviderExtensions
    {
        /// <summary>
        /// Creates a logger with the name of the given <see cref="TName"/> type.
        /// </summary>
        /// <typeparam name="TName">The type of the class to be used as name of the logger.</typeparam>
        /// <param name="self">An ILoggerProvider instance.</param>
        /// <returns>An instance of the <see cref="ILogger"/>.</returns>
        public static ILogger CreateLogger<TName>(this ILoggerProvider self) where TName : class
        {
            if (self == null)
            {
                throw new ArgumentNullException(nameof(self));
            }

            if (!self.GetType().IsAssignableFrom(typeof(Log4NetProvider)))
            {
                throw new ArgumentOutOfRangeException(nameof(self), $"The {nameof(ILoggerProvider)} should be of type {nameof(Log4NetProvider)}.");
            }

            return self.CreateLogger(typeof(TName).FullName);
        }
    }
}