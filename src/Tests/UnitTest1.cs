using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Tests
{
    [TestClass]
    public class LoggerShould
    {
        [TestMethod]
        public void TestMethod1()
        {
			var provider = new Log4NetProvider("log4net.config", null);
			var logger = provider.CreateLogger("Test");

			logger.LogCritical("A message");

			Assert.Inconclusive();
        }

		[TestMethod]
		public void UsePatternLayoutOnExceptions()
		{
			var provider = new Log4NetProvider("log4net.config", null);
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

		private void ThrowException()
		{
			throw new InvalidOperationException("A message");
		}
    }
}
