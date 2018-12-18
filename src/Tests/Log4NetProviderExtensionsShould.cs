namespace NetCore2.Tests
{
	using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Extensions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class Log4NetProviderExtensionsShould
	{
		[TestMethod]
		public void CreateLoggerWithTypeName()
		{
			var provider = new Log4NetProvider();

			Log4NetLogger logger = provider.CreateLogger<Log4NetProviderExtensionsShould>() as Log4NetLogger;

			Assert.IsNotNull(logger);
			Assert.AreEqual(typeof(Log4NetProviderExtensionsShould).FullName, logger.Name);
		}
	}
}