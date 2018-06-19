namespace Tests
{
	using System;
	using System.IO;

	using Microsoft.Extensions.Logging;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class LoggerShould
	{

		[TestMethod]
		public void ProviderShouldBeCreatedWithConfigurationSectionOverrides()
		{
			var builder = new ConfigurationBuilder();
			builder.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json");
			var configuration = builder.Build();
			var provider = new Log4NetProvider("log4net.config", configuration.GetSection("Logging"));
		}

		[TestMethod]
		public void LogCriticalMessages()
		{
			var provider = new Log4NetProvider("log4net.config");
			var logger = provider.CreateLogger("Test");

			logger.LogCritical("A message");

			Assert.Inconclusive();
		}

		[TestMethod]
		public void UsePatternLayoutOnExceptions()
		{
			var provider = new Log4NetProvider("log4net.config");
			var logger = provider.CreateLogger("Test");

			try
			{
				ThrowException();
			}
			catch (Exception ex)
			{
				logger.LogCritical(10, ex, "Catched message");
			}

			Assert.Inconclusive();
		}

		/// <summary>
		/// Throws the exception, and have stacktrace to be tested by the ExceptionLayoutPattern.
		/// </summary>
		/// <exception cref="InvalidOperationException">A message</exception>
		private static void ThrowException() => throw new InvalidOperationException("A message");
	}
}