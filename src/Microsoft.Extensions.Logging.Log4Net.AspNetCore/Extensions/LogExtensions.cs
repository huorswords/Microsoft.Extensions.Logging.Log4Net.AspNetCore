namespace Microsoft.Extensions.Logging.Log4Net.AspNetCore.Extensions
{
	using System;

	using log4net;

	/// <summary>
	/// The <see cref="ILogExtensions"/> class.
	/// </summary>
	public static class LogExtensions
	{
		/// <summary>
		/// Criticals the specified message.
		/// </summary>
		/// <param name="log">The log.</param>
		/// <param name="message">The message.</param>
		/// <param name="exception">The exception.</param>
		public static void Critical(this ILog log, object message, Exception exception)
			=> log.Logger.Log(null, log4net.Core.Level.Critical, message, exception);

		/// <summary>
		/// Traces the specified message.
		/// </summary>
		/// <param name="log">The log.</param>
		/// <param name="message">The message.</param>
		/// <param name="exception">The exception.</param>
		public static void Trace(this ILog log, object message, Exception exception)
			=> log.Logger.Log(null, log4net.Core.Level.Trace, message, exception);
	}
}