using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Log4Net.AspNetCore.Entities;
using NetFrameworkv4_7_2.Tests.Listeners;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;

namespace NetFrameworkv4_7_2.Tests
{
    public class LoggerShould
    {
        private const string Log4NetConfigFileName = "log4net.config";

        private const string ScopedKeyText = "test";

        private const string ScopedValueText = "TEST_SCOPE";

        private const string TestLoggerName = "Test";

        private const string MessageText = "A message";

        private const string LogFileOverrideName = "overridOH.log";

        private CustomTraceListener listener;

        public LoggerShould()
        {
            this.listener = new CustomTraceListener();
            Trace.Listeners.Add(listener);
        }

        [Fact]
        public void Provider_Should_InitializeLogging_When_UsingAppConfigFile()
        {
            const string message = MessageText;
            var options = new Log4NetProviderOptions
            {
                UseWebOrAppConfig = true
            };

            using (var provider = new Log4NetProvider(options))
            {
                var logger = provider.CreateLogger(TestLoggerName);
                logger.LogCritical(message);
            }

            List<string> messages = listener.Messages.ToList();
            Assert.Single(messages);
            Assert.NotNull(messages.FirstOrDefault(x => x.Contains(message)));
            Assert.NotNull(messages.FirstOrDefault(x => x.StartsWith($"{TestLoggerName}: APP.CONFIG")));
        }

        [Fact]
        public void Include_ScopePropertyOnMessages_When_ScopeIsString()
        {
            var provider = new Log4NetProvider(Log4NetConfigFileName);
            var logger = provider.CreateLogger(TestLoggerName);

            const string message = MessageText;
            using (var scope = logger.BeginScope(ScopedValueText))
            {
                logger.LogCritical(message);
            }

            Assert.Equal(1, this.listener.Messages.Count);
            Assert.Contains(this.listener.Messages, x => x.Contains(message));
            Assert.Contains(this.listener.Messages, x => x.Contains(ScopedValueText));
        }

        [Fact]
        public void Include_ScopePropertyOnMessages_When_ScopeIsDictionaryOfObjects_And_AnyValueIsNull()
        {
            var provider = new Log4NetProvider(Log4NetConfigFileName);
            var logger = provider.CreateLogger(TestLoggerName);

            using (var scope = logger.BeginScope(new Dictionary<string, object>() { { ScopedKeyText, null } }))
            {
                logger.LogCritical(MessageText);
            }

            Assert.Equal(1, this.listener.Messages.Count);
            Assert.Contains(this.listener.Messages, x => x.Contains(MessageText));
            Assert.Contains(this.listener.Messages, x => x.Contains($"(null) (null) MESSAGE: {MessageText}"));
        }

        [Fact]
        public void Include_ScopePropertyOnMessages_When_ScopeIsDictionaryOfObjects()
        {
            var provider = new Log4NetProvider(Log4NetConfigFileName);
            var logger = provider.CreateLogger(TestLoggerName);

            using (var scope = logger.BeginScope(new Dictionary<string, object>() { { ScopedKeyText, ScopedValueText } }))
            {
                logger.LogCritical(MessageText);
            }

            Assert.Equal(1, this.listener.Messages.Count);
            Assert.Contains(this.listener.Messages, x => x.Contains(MessageText));
            Assert.Contains(this.listener.Messages, x => x.Contains(ScopedValueText));
        }

        [Fact]
        public void Include_ScopePropertyOnMessages_When_ScopeIsDictionaryOfStrings()
        {
            var provider = new Log4NetProvider(Log4NetConfigFileName);
            var logger = provider.CreateLogger(TestLoggerName);

            const string message = MessageText;
            using (var scope = logger.BeginScope(new Dictionary<string, string>() { { ScopedKeyText, ScopedValueText } }))
            {
                logger.LogCritical(message);
            }

            Assert.Equal(1, this.listener.Messages.Count);
            Assert.Contains(this.listener.Messages, x => x.Contains(message));
            Assert.Contains(this.listener.Messages, x => x.Contains(ScopedValueText));
        }

        [Fact]
        public void ProviderShouldBeCreatedWithOptions()
        {
            const string OverridOHLogFilePath = LogFileOverrideName;
            if (File.Exists(OverridOHLogFilePath))
            {
                File.Delete(OverridOHLogFilePath);
            }

            var options = GetLog4NetProviderOptions();
            var provider = new Log4NetProvider(options);
            var logger = provider.CreateLogger();
            logger.LogCritical(MessageText);

            Assert.NotNull(provider);
            Assert.True(File.Exists(OverridOHLogFilePath));
        }

        [Fact]
        public void LogCriticalMessages()
        {
            var provider = new Log4NetProvider(Log4NetConfigFileName);
            var logger = provider.CreateLogger(TestLoggerName);

            const string message = MessageText;
            logger.LogCritical(message);

            Assert.Equal(1, this.listener.Messages.Count);
            Assert.Contains(this.listener.Messages, x => x.Contains(message));
        }

        [Fact]
        public void ProviderShouldCreateLoggerUsingConfigurationFileRelativePath()
        {
            var provider = new Log4NetProvider(Log4NetConfigFileName);

            var logger = provider.CreateLogger(TestLoggerName);

            const string message = MessageText;
            logger.LogCritical(message);

            Assert.Equal(1, this.listener.Messages.Count);
            Assert.Contains(this.listener.Messages, x => x.Contains(message));
        }

        [Fact]
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

            Assert.Equal(1, this.listener.Messages.Count);
            Assert.Contains(listener.Messages, x => x.Contains(Message));
        }

        /// <summary>
        /// Throws the exception, and have stacktrace to be tested by the ExceptionLayoutPattern.
        /// </summary>
        /// <exception cref="InvalidOperationException">A message</exception>
        private static void ThrowException() => throw new InvalidOperationException(MessageText);

        /// <summary>
        /// Gets the log4net provider options.
        /// </summary>
        /// <returns></returns>
        private static Log4NetProviderOptions GetLog4NetProviderOptions()
        {
            var attributes = new Dictionary<string, string>
            {
                { "Value", LogFileOverrideName }
            };

            var nodeInfo = new NodeInfo { XPath = "" };
            var builder = new Log4NetProviderOptions()
            {
                PropertyOverrides = new List<NodeInfo>
                {
                    new NodeInfo {
                        XPath = "/log4net/appender[@name='RollingFile']/file",
                        Attributes = attributes
                    }
                }
            };

            return builder;
        }
    }
}