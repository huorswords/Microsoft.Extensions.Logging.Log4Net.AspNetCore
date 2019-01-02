namespace Microsoft.Extensions.Logging
{
	using System;

	using log4net;

	using Microsoft.Extensions.Logging.Log4Net.AspNetCore.Extensions;

	/// <summary>
	/// The log4net logger class.
	/// </summary>
	public class Log4NetLogger : ILogger
	{
		/// <summary>
		/// The log.
		/// </summary>
		private readonly ILog log;

		/// <summary>
		/// The provider options.
		/// </summary>
		private readonly Log4NetProviderOptions options;

		/// <summary>
		/// Initializes a new instance of the <see cref="Log4NetLogger"/> class.
		/// </summary>
		/// <param name="loggerRepository">The repository name.</param>
		/// <param name="name">The logger's name.</param>
		[Obsolete("Use Log4NetLogger(Log4NetLoggerOptions) instead")]
		public Log4NetLogger(string loggerRepository, string name)
			=> this.log = LogManager.GetLogger(loggerRepository, name);

		/// <summary>
		/// Initializes a new instance of the <see cref="Log4NetLogger"/> class.
		/// </summary>
		/// <param name="options">The log4net provider options.</param>
		public Log4NetLogger(Log4NetProviderOptions options)
		{
			this.options = options ?? throw new ArgumentNullException(nameof(options));
			this.log = LogManager.GetLogger(options.LoggerRepository, options.Name);
		}

		/// <summary>
		/// Gets the name.
		/// </summary>
		public string Name
			=> this.log.Logger.Name;

		/// <summary>
		/// Begins a logical operation scope.
		/// </summary>
		/// <typeparam name="TState">The type of the state.</typeparam>
		/// <param name="state">The identifier for the scope.</param>
		/// <returns>
		/// An IDisposable that ends the logical operation scope on dispose.
		/// </returns>
		public IDisposable BeginScope<TState>(TState state)
			=> options.ScopeFactory?.BeginScope(state);

		/// <summary>
		/// Determines whether the logging level is enabled.
		/// </summary>
		/// <param name="logLevel">The log level.</param>
		/// <returns>The <see cref="bool"/> value indicating whether the logging level is enabled.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Throws when <paramref name="logLevel"/> is outside allowed range</exception>
		public bool IsEnabled(LogLevel logLevel)
		{
			switch (logLevel)
			{
				case LogLevel.Critical:
					return this.log.IsFatalEnabled;
				case LogLevel.Debug:
				case LogLevel.Trace:
					return this.log.IsDebugEnabled;
				case LogLevel.Error:
					return this.log.IsErrorEnabled;
				case LogLevel.Information:
					return this.log.IsInfoEnabled;
				case LogLevel.Warning:
					return this.log.IsWarnEnabled;
				default:
					throw new ArgumentOutOfRangeException(nameof(logLevel));
			}
		}

		/// <summary>
		/// Logs an exception into the log.
		/// </summary>
		/// <param name="logLevel">The log level.</param>
		/// <param name="eventId">The event Id.</param>
		/// <param name="state">The state.</param>
		/// <param name="exception">The exception.</param>
		/// <param name="formatter">The formatter.</param>
		/// <typeparam name="TState">The type of the state.</typeparam>
		/// <exception cref="ArgumentNullException">Throws when the <paramref name="formatter"/> is null.</exception>
		public void Log<TState>(
			LogLevel logLevel,
			EventId eventId,
			TState state,
			Exception exception,
			Func<TState, Exception, string> formatter)
		{
			if (!this.IsEnabled(logLevel))
			{
				return;
			}

			if (formatter == null)
			{
				throw new ArgumentNullException(nameof(formatter));
			}

			string message = formatter(state, exception);
			if (!string.IsNullOrEmpty(message)
				|| exception != null)
			{
				switch (logLevel)
				{
					case LogLevel.Critical:
						string overrideCriticalLevelWith = options.OverrideCriticalLevelWith;
						if (!string.IsNullOrEmpty(overrideCriticalLevelWith)
							&& overrideCriticalLevelWith.Equals(LogLevel.Critical.ToString(), StringComparison.OrdinalIgnoreCase))
						{
							this.log.Critical(message, exception);
						}
						else
						{
							this.log.Fatal(message, exception);
						}

						break;

					case LogLevel.Debug:
						this.log.Debug(message, exception);
						break;

					case LogLevel.Error:
						this.log.Error(message, exception);
						break;

					case LogLevel.Information:
						this.log.Info(message, exception);
						break;

					case LogLevel.Warning:
						this.log.Warn(message, exception);
						break;

					case LogLevel.Trace:
						this.log.Trace(message, exception);
						break;

					default:
						this.log.Warn($"Encountered unknown log level {logLevel}, writing out as Info.");
						this.log.Info(message, exception);
						break;
				}
			}
		}
	}
}