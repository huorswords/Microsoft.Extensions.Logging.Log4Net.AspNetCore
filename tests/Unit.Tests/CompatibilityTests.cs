using FluentAssertions;
using log4net;
using log4net.Appender;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Unit.Tests
{
    [Collection("AppenderCollection")]
    public class CompatibilityTests
    {
        [Fact]
        public void RelativeConfigPath_Should_Resolve_From_AppContextBaseDirectory()
        {
            var repositoryName = $"compat-relative-{Guid.NewGuid():N}";
            var fileName = $"compat-{Guid.NewGuid():N}.config";
            var filePath = Path.Combine(AppContext.BaseDirectory, fileName);

            File.WriteAllText(filePath, CreateTestConfigXml());

            try
            {
                using (var provider = new Log4NetProvider(new Log4NetProviderOptions(fileName)
                {
                    LoggerRepository = repositoryName,
                    Name = repositoryName,
                }))
                {
                    var logger = provider.CreateLogger(repositoryName);
                    logger.LogInformation("hello from relative path");

                    var appender = GetTestAppender(repositoryName);
                    appender.GetEvents().Should().ContainSingle();
                    appender.GetEvents().Single().RenderedMessage.Should().Contain("hello from relative path");
                }
            }
            finally
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }

        [Fact]
        public void Provider_Should_Log_WithoutThrowing_On_CurrentRuntime()
        {
            var repositoryName = $"compat-smoke-{Guid.NewGuid():N}";
            var filePath = CreateTemporaryConfigFile();

            try
            {
                using (var provider = new Log4NetProvider(new Log4NetProviderOptions(filePath)
                {
                    LoggerRepository = repositoryName,
                    Name = repositoryName,
                }))
                {
                    var logger = provider.CreateLogger(repositoryName);
                    Action action = () => logger.LogWarning("smoke test message");

                    action.Should().NotThrow();

                    var appender = GetTestAppender(repositoryName);
                    appender.GetEvents().Should().ContainSingle();
                    appender.GetEvents().Single().RenderedMessage.Should().Contain("smoke test message");
                }
            }
            finally
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }

        private static MemoryAppender GetTestAppender(string repositoryName)
        {
            var repository = LogManager.GetRepository(repositoryName);

            return repository.GetAppenders()
                .OfType<MemoryAppender>()
                .Single(appender => appender.Name == "TestAppender");
        }

        private static string CreateTemporaryConfigFile()
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, $"compat-{Guid.NewGuid():N}.config");
            File.WriteAllText(filePath, CreateTestConfigXml());
            return filePath;
        }

        private static string CreateTestConfigXml()
            => @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<log4net>
  <appender name=""TestAppender"" type=""log4net.Appender.MemoryAppender"">
    <layout type=""log4net.Layout.PatternLayout"">
      <conversionPattern value=""%message%newline"" />
    </layout>
  </appender>
  <root>
    <level value=""ALL"" />
    <appender-ref ref=""TestAppender"" />
  </root>
</log4net>";
    }
}