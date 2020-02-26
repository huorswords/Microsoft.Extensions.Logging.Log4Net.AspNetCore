using log4net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Extensions;
using System.Reflection;
using Xunit;

namespace NetFrameworkv4_7_2.Tests
{
    public class Log4NetProviderExtensionsShould
    {
        [Fact]
        public void CreateLoggerWithTypeName()
        {
            var provider = new Log4NetProvider();

            Log4NetLogger logger = provider.CreateLogger<Log4NetProviderExtensionsShould>() as Log4NetLogger;

            Assert.NotNull(logger);
            Assert.Equal(typeof(Log4NetProviderExtensionsShould).FullName, logger.Name);
        }

        [Fact]
        public void CreateDefaultLoggerWithoutTypeName()
        {
            var provider = new Log4NetProvider();

            var logger = provider.CreateLogger() as Log4NetLogger;

            Assert.NotNull(logger);
            Assert.Equal(string.Empty, logger.Name);
        }

        [Fact]
        public void WhenLoggerShouldBeExternallyConfigured_RepositoryIsNotConfigured()
        {
            LogManager.ResetConfiguration(Assembly.GetExecutingAssembly());

            new Log4NetProvider(new Log4NetProviderOptions
            {
                ExternalConfigurationSetup = true
            });

            var repository = LogManager.GetRepository(Assembly.GetExecutingAssembly());
            Assert.False(repository.Configured);
        }

        [Fact]
        public void WhenLoggerShouldNotBeExternallyConfigured_RepositoryIsConfigured()
        {
            new Log4NetProvider(new Log4NetProviderOptions());

            var repository = LogManager.GetRepository(Assembly.GetExecutingAssembly());
            Assert.True(repository.Configured);
        }

        [Fact]
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