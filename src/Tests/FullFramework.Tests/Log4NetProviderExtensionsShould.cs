namespace FullFramework.Tests
{
    using System.Reflection;
        
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Extensions;

    using log4net;

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
        [TestMethod]
        public void CreateDefaultLoggerWithoutTypeName()
        {
            var provider = new Log4NetProvider();

            var logger = provider.CreateLogger() as Log4NetLogger;

            Assert.IsNotNull(logger);
            Assert.AreEqual(string.Empty, logger.Name);
        }

        [TestMethod]
        public void WhenLoggerShouldBeExternallyConfigured_RepositoryIsNotConfigured()
        {
            LogManager.ResetConfiguration(Assembly.GetExecutingAssembly());

            new Log4NetProvider(new Log4NetProviderOptions
            {
                ExternalConfigurationSetup = true
            });

            var repository = LogManager.GetRepository(Assembly.GetExecutingAssembly());
            Assert.IsFalse(repository.Configured);
        }

        [TestMethod]
        public void WhenLoggerShouldNotBeExternallyConfigured_RepositoryIsConfigured()
        {
            new Log4NetProvider(new Log4NetProviderOptions());

            var repository = LogManager.GetRepository(Assembly.GetExecutingAssembly());
            Assert.IsTrue(repository.Configured);
        }

        [TestMethod]
        public void WhenRepositoryNameIsGivenButRepositoryIsAlreadyCreated_ProviderUsesAlreadyCreatedRepository()
        {
            LogManager.CreateRepository("abc");

            new Log4NetProvider(new Log4NetProviderOptions
            {
                LoggerRepository = "abc"
            });
        }
    }
}