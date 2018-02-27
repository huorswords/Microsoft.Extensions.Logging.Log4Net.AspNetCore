using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }
}
