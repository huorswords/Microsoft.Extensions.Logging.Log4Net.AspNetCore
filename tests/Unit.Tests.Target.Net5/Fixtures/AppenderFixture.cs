using FluentAssertions;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Repository;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Unit.Tests.Target.Netcore31.Models;
using Xunit;

namespace Unit.Tests.Target.Netcore31.Fixtures
{
    [CollectionDefinition("AppenderCollection")]
    public class AppenderCollection : ICollectionFixture<AppenderFixture>
    {
    }

    public class AppenderFixture
    {
        public AppenderFixture()
        {
        }

        public MemoryAppender GetTestAppender()
        {
            var appender = CreateTestAppender();
            appender.Should()
                    .BeOfType<MemoryAppender>();
            MemoryAppender testAppender = (MemoryAppender)appender;
            return testAppender;
        }

        public MemoryAppender GetTestAppender(Log4NetProviderOptions options)
        {
            var appender = CreateTestAppender(options);
            appender.Should()
                    .BeOfType<MemoryAppender>();
            MemoryAppender testAppender = (MemoryAppender)appender;
            return testAppender;
        }

        private static IAppender CreateTestAppender()
        {
            ILoggerRepository repository = GetOrCreateRepository();

            return repository.GetAppenders()
                             .Where(x => x.Name.Equals("TestAppender", StringComparison.InvariantCultureIgnoreCase))
                             .FirstOrDefault();
        }

        private static IAppender CreateTestAppender(Log4NetProviderOptions options)
        {
            ILoggerRepository repository = GetOrCreateRepository(options);

            return repository.GetAppenders()
                             .Where(x => x.Name.Equals("TestAppender", StringComparison.InvariantCultureIgnoreCase))
                             .FirstOrDefault();
        }

        private static ILoggerRepository GetOrCreateRepository()
        {
            var repositoryName = "log4net-default-repository";
            ILoggerRepository repository = null;
            var repositories = LogManager.GetAllRepositories();
            if (repositories.Any(x => x.Name.Equals(repositoryName, StringComparison.InvariantCultureIgnoreCase)))
            {
                repository = LogManager.GetRepository(repositoryName);
            }
            else
            {
                repository = LogManager.CreateRepository(repositoryName);
            }

            return repository;
        }

        private static ILoggerRepository GetOrCreateRepository(Log4NetProviderOptions options)
        {
            ILoggerRepository repository = null;
            var repositories = LogManager.GetAllRepositories();
            if (repositories.Any(x => x.Name.Equals(options.LoggerRepository, StringComparison.InvariantCultureIgnoreCase)))
            {
                repository = LogManager.GetRepository(options.LoggerRepository);
            }
            else
            {
                repository = LogManager.CreateRepository(options.LoggerRepository);
            }

            return repository;
        }

        public Log4NetProviderOptions ConfigureOptions(Log4NetFileOption log4NetFile)
        {
            const string RepositoryName = "Test";

            var options = new Log4NetProviderOptions()
            {
                Log4NetConfigFileName = GetLog4netFilePath(log4NetFile),
                LoggerRepository = RepositoryName,
                Name = RepositoryName,
                LoggingEventFactory = new Log4NetLoggingEventFactory(),
                LogLevelTranslator = new Log4NetLogLevelTranslator(),
            };

            SetupLog4NetRepository(options);

            return options;
        }

        public string GetLog4netFilePath(Log4NetFileOption log4NetFile)
            => $"Resources/log4net.{log4NetFile}.config";

        private static void SetupLog4NetRepository(Log4NetProviderOptions options)
        {
            ILoggerRepository repository = GetOrCreateRepository(options);

            var assemblyFile = new FileInfo(Assembly.GetCallingAssembly().GetAssemblyLocation());
            string path = Path.Combine(assemblyFile.Directory.FullName, options.Log4NetConfigFileName);

            XmlConfigurator.Configure(repository, File.OpenRead(path));
        }
    }
}