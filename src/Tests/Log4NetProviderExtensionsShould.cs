namespace Tests
{
	using Microsoft.Extensions.Logging;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class Log4NetProviderExtensionsShould
	{
		[TestMethod]
		public void CreateLoggerWithTypeName()
		{
			var provider = new Log4NetProvider("log4net.config");

			Log4NetLogger logger = provider.CreateLogger<Log4NetProviderExtensionsShould>() as Log4NetLogger;

			Assert.IsNotNull(logger);
			Assert.AreEqual("Tests.Log4NetProviderExtensionsShould", logger.Name);
		}
	}
}