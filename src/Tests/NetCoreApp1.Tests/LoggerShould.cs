namespace NetCoreApp.v1_x.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using NetCore1.Tests.Listeners;

    [TestClass]
    public class LoggerShould
    {
        private const string Log4NetConfigFileName = "log4net.config";

        private const string MessageText = "A message";

        private const string TestLoggerName = "Test";

        private const string ScopedValueText = "TEST_SCOPE";

        private const string ScopedKeyText = "test";

        private CustomTraceListener listener;

        [TestInitialize]
        public void Setup()
        {
            this.listener = new CustomTraceListener();
            Trace.Listeners.Add(listener);
        }

        [TestMethod]
        public void Include_ScopePropertyOnMessages_When_ScopeIsString()
        {
            var provider = new Log4NetProvider(Log4NetConfigFileName);
            var logger = provider.CreateLogger(TestLoggerName);

            using (var scope = logger.BeginScope(ScopedValueText))
            {
                logger.LogCritical(MessageText);
            }

            Assert.AreEqual(1, this.listener.Messages.Count);
            Assert.IsTrue(this.listener.Messages.Any(x => x.Contains(MessageText)));
            Assert.IsTrue(this.listener.Messages.Any(x => x.Contains(ScopedValueText)));
        }

        [TestMethod]
        public void Include_ScopePropertyOnMessages_When_ScopeIsDictionaryOfObjects_And_AnyValueIsNull()
        {
            var provider = new Log4NetProvider(Log4NetConfigFileName);
            var logger = provider.CreateLogger(TestLoggerName);

            using (var scope = logger.BeginScope(new Dictionary<string, object>() { { ScopedKeyText, null } }))
            {
                logger.LogCritical(MessageText);
            }

            Assert.AreEqual(1, this.listener.Messages.Count);
            Assert.IsTrue(this.listener.Messages.Any(x => x.Contains(MessageText)));
            Assert.IsTrue(this.listener.Messages.Any(x => x.Contains($"(null) (null) MESSAGE: {MessageText}")));
        }

        [TestMethod]
        public void Include_ScopePropertyOnMessages_When_ScopeIsDictionaryOfObjects()
        {
            var provider = new Log4NetProvider(Log4NetConfigFileName);
            var logger = provider.CreateLogger(TestLoggerName);

            using (var scope = logger.BeginScope(new Dictionary<string, object>() { { ScopedKeyText, ScopedValueText } }))
            {
                logger.LogCritical(MessageText);
            }

            Assert.AreEqual(1, this.listener.Messages.Count);
            Assert.IsTrue(this.listener.Messages.Any(x => x.Contains(MessageText)));
            Assert.IsTrue(this.listener.Messages.Any(x => x.Contains(ScopedValueText)));
        }

        [TestMethod]
        public void Include_ScopePropertyOnMessages_When_ScopeIsDictionaryOfStrings()
        {
            var provider = new Log4NetProvider(Log4NetConfigFileName);
            var logger = provider.CreateLogger(TestLoggerName);

            using (var scope = logger.BeginScope(new Dictionary<string, string>() { { ScopedKeyText, ScopedValueText } }))
            {
                logger.LogCritical(MessageText);
            }

            Assert.AreEqual(1, this.listener.Messages.Count);
            Assert.IsTrue(this.listener.Messages.Any(x => x.Contains(MessageText)));
            Assert.IsTrue(this.listener.Messages.Any(x => x.Contains(ScopedValueText)));
        }

        [TestMethod]
        public void ProviderShouldBeCreatedWithOptions()
        {
            const string OverridOHLogFilePath = "overridOH.log";
            if (File.Exists(OverridOHLogFilePath))
            {
                File.Delete(OverridOHLogFilePath);
            }

            var options = GetLog4NetProviderOptions();
            var provider = new Log4NetProvider(options);
            var logger = provider.CreateLogger();
            logger.LogCritical(MessageText);

            Assert.IsNotNull(provider);
            Assert.IsTrue(File.Exists(OverridOHLogFilePath));
        }

        [TestMethod]
        public void LogCriticalMessages()
        {
            var provider = new Log4NetProvider(Log4NetConfigFileName);
            var logger = provider.CreateLogger(TestLoggerName);

            const string message = MessageText;
            logger.LogCritical(message);

            Assert.AreEqual(1, this.listener.Messages.Count);
            Assert.IsTrue(this.listener.Messages.Any(x => x.Contains(message)));
        }

        [TestMethod]
        public void ProviderShouldCreateLoggerUsingConfigurationFileRelativePath()
        {
            var provider = new Log4NetProvider(Log4NetConfigFileName);

            var logger = provider.CreateLogger(TestLoggerName);

            const string message = MessageText;
            logger.LogCritical(message);

            Assert.AreEqual(1, this.listener.Messages.Count);
            Assert.IsTrue(this.listener.Messages.Any(x => x.Contains(message)));
        }

        [TestMethod]
        public void UsePatternLayoutOnExceptions()
        {
            const string Message = "Catched message";

            var provider = new Log4NetProvider(Log4NetConfigFileName);
            var logger = provider.CreateLogger(TestLoggerName);
            
            try
            {
                ThrowException();
            }
            catch (Exception ex)
            {
                logger.LogCritical(10, ex, Message);
            }

            Assert.AreEqual(1, this.listener.Messages.Count);
            Assert.IsTrue(listener.Messages.Any(x => x.Contains(Message)));
        }

        /// <summary>
        /// Throws the exception, and have stacktrace to be tested by the ExceptionLayoutPattern.
        /// </summary>
        /// <exception cref="InvalidOperationException">A message</exception>
        private static void ThrowException() => throw new InvalidOperationException(MessageText);

        /// <summary>
        /// Gets the .Net Core configuration.
        /// </summary>
        /// <returns>The <see cref="IConfigurationRoot"/> of the appsettings.json file.</returns>
        private static IConfigurationRoot GetNetCoreConfiguration()
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json");
            var configuration = builder.Build();
            return configuration;
        }

        /// <summary>
        /// Gets the log4net provider options.
        /// </summary>
        /// <returns></returns>
        private static Log4NetProviderOptions GetLog4NetProviderOptions()
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            var configuration = builder.Build();

            return configuration.GetSection("Log4NetCore").Get<Log4NetProviderOptions>();
        }
    }
}