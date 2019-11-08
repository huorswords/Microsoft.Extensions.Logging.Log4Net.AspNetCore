namespace Swords.Core.Tests
{
	using System;
    using System.Collections.Generic;
    using System.Diagnostics;
	using System.IO;
	using System.Linq;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Swords.Core.Tests.Listeners;

    [TestClass]
	public class LoggerShould
	{
		private const string DefaultLog4NetConfigFileName = "log4net.config";

        private const string TestLoggerName = "Test";
        
        private const string MessageText = "A message";

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
            var provider = new Log4NetProvider(DefaultLog4NetConfigFileName);
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
            var provider = new Log4NetProvider(DefaultLog4NetConfigFileName);
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
            var scopedValue = GetTestScopedKeyValuePair();
            var scopedDictionary = new Dictionary<string, string>
            {
                { scopedValue.Key, scopedValue.Value }
            };

            var provider = new Log4NetProvider(DefaultLog4NetConfigFileName);
            var logger = provider.CreateLogger(TestLoggerName);

            using (var scope = logger.BeginScope(scopedDictionary))
            {
                logger.LogCritical(MessageText);
            }

            Assert.AreEqual(1, this.listener.Messages.Count);
            Assert.IsTrue(this.listener.Messages.Any(x => x.Contains(MessageText)));
            Assert.IsTrue(this.listener.Messages.Any(x => x.Contains(scopedValue.Value)));
        }

        [TestMethod]
        public void Include_ScopePropertyOnMessages_When_ScopeIsDictionaryOfStrings()
        {
            var scopedValue = GetTestScopedKeyValuePair();
            var scopedDictionary = new Dictionary<string, string>
            {
                { scopedValue.Key, scopedValue.Value }
            };

            var provider = new Log4NetProvider(DefaultLog4NetConfigFileName);
            var logger = provider.CreateLogger(TestLoggerName);

            using (var scope = logger.BeginScope(scopedDictionary))
            {
                logger.LogCritical(MessageText);
            }

            Assert.AreEqual(1, this.listener.Messages.Count);
            Assert.IsTrue(this.listener.Messages.Any(x => x.Contains(MessageText)));
            Assert.IsTrue(this.listener.Messages.Any(x => x.Contains(scopedValue.Value)));
        }

		[TestMethod]
		public void LogCriticalMessages()
		{
			var provider = new Log4NetProvider(DefaultLog4NetConfigFileName);
			var logger = provider.CreateLogger(TestLoggerName);

			logger.LogCritical(MessageText);

			Assert.AreEqual(1, this.listener.Messages.Count);
			Assert.IsTrue(this.listener.Messages.Any(x => x.Contains(MessageText)));
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
		public void ProviderShouldCreateLoggerUsingConfigurationFileRelativePath()
		{
			var provider = new Log4NetProvider(DefaultLog4NetConfigFileName);

			var logger = provider.CreateLogger(TestLoggerName);

			logger.LogCritical(MessageText);

			Assert.AreEqual(1, this.listener.Messages.Count);
			Assert.IsTrue(this.listener.Messages.Any(x => x.Contains(MessageText)));
		}

		[TestMethod]
		public void UsePatternLayoutOnExceptions()
		{
            const string CatchedMessageText = "Catched message";

            var provider = new Log4NetProvider(DefaultLog4NetConfigFileName);
			var logger = provider.CreateLogger(TestLoggerName);

			try
			{
				ThrowException();
			}
			catch (Exception ex)
			{
				logger.LogCritical(10, ex, CatchedMessageText);
			}

			Assert.AreEqual(1, this.listener.Messages.Count);
			Assert.IsTrue(this.listener.Messages.Any(x => x.Contains(CatchedMessageText)));
		}

        private static KeyValuePair<string, string> GetTestScopedKeyValuePair() 
            => new KeyValuePair<string, string>(ScopedKeyText, ScopedValueText);

        /// <summary>
        /// Throws the exception, and have stacktrace to be tested by the ExceptionLayoutPattern.
        /// </summary>
        /// <exception cref="InvalidOperationException">A message</exception>
        private static void ThrowException()
			=> throw new InvalidOperationException(MessageText);

        protected virtual IConfigurationRoot GetNetCoreConfiguration()
            => throw new NotImplementedException();

		/// <summary>
		/// Gets the log4net provider options.
		/// </summary>
		/// <returns></returns>
		protected virtual Log4NetProviderOptions GetLog4NetProviderOptions()
            => throw new NotImplementedException();
    }
}