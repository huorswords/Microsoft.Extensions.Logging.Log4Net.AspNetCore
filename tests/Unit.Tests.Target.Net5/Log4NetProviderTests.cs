using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Unit.Tests.Target.Net5.Fixtures;
using Xunit;

namespace Unit.Tests.Target.Net5
{
    [Collection("AppenderCollection")]
    public class Log4NetProviderTests
    {
        public Log4NetProviderTests(AppenderFixture context)
        {
            Context = context;
        }

        private AppenderFixture Context { get; }

        [Fact]
        public void Ctor_Should_CreateInstance_That_AllowCreateLogger()
        {
            var sut = new Log4NetProvider();

            var logger = sut.CreateLogger();
            logger.Should().BeOfType(typeof(Log4NetLogger));
        }

        [Fact]
        public void Ctor_When_FilenameArgument_Should_CreateInstance_That_AllowCreateLogger()
        {
            var sut = new Log4NetProvider(Context.GetLog4netFilePath(Models.Log4NetFileOption.All));

            var logger = sut.CreateLogger();
            logger.Should().BeOfType(typeof(Log4NetLogger));
        }
        

        [Fact]
        public void WhenLoggingEventFactoryIsNullOnProviderOptions_ThenDefaultLog4NetLoggingEventFactoryIsUsed()
        {
            var options = new Log4NetProviderOptions
            {
                LoggingEventFactory = null
            };

            var sut = new Log4NetProvider(options);
            var logger = sut.CreateLogger("test") as Log4NetLogger;

            var internalOptions = GetInternalOptions(logger);

            internalOptions.Should().NotBeNull();
            internalOptions.LoggingEventFactory.Should().NotBeNull("a default LoggingEventFactory is needed to create LoggingEvents")
                                                        .And.BeOfType<Log4NetLoggingEventFactory>("because this is the default factory type");
        }

        [Fact]
        public void WhenLoggingEventFactoryIsProvidedInProviderOptions_ThenLoggerUsesProvidedLoggingEventFactory()
        {
            var expectedFactory = new Log4NetLoggingEventFactory();
            var options = new Log4NetProviderOptions
            {
                LoggingEventFactory = expectedFactory
            };

            var sut = new Log4NetProvider(options);
            var logger = sut.CreateLogger("test") as Log4NetLogger;

            var internalOptions = GetInternalOptions(logger);

            internalOptions.Should().NotBeNull();
            internalOptions.LoggingEventFactory.Should().NotBeNull("a LoggingEventFactory was provided in the options")
                                                        .And.Be(expectedFactory, "this LoggingEventFactory was provided in the options");
        }

        [Fact]
        public void WhenLogLevelTranslatorIsNullOnProviderOptions_ThenDefaultLog4NetLogLevelTranslatorIsUsed()
        {
            var options = new Log4NetProviderOptions
            {
                LogLevelTranslator = null
            };

            var sut = new Log4NetProvider(options);
            var logger = sut.CreateLogger("test") as Log4NetLogger;

            var internalOptions = GetInternalOptions(logger);

            internalOptions.Should().NotBeNull();
            internalOptions.LogLevelTranslator.Should().NotBeNull("a default LogLevelTranslator is needed to create LoggingEvents")
                                                        .And.BeOfType<Log4NetLogLevelTranslator>("because this is the default translator type");
        }

        [Fact]
        public void WhenLogLevelTranslatorIsProvidedInProviderOptions_ThenLoggerUsesProvidedLogLevelTranslator()
        {
            var expectedTranslator = new Log4NetLogLevelTranslator();
            var options = new Log4NetProviderOptions
            {
                LogLevelTranslator = expectedTranslator
            };

            var sut = new Log4NetProvider(options);
            var logger = sut.CreateLogger("test") as Log4NetLogger;

            var internalOptions = GetInternalOptions(logger);

            internalOptions.Should().NotBeNull();
            internalOptions.LogLevelTranslator.Should().NotBeNull("a LogLevelTranslator was provided in the options")
                                                        .And.Be(expectedTranslator, "this LogLevelTranslator was provided in the options");
        }

        private Log4NetProviderOptions GetInternalOptions(Log4NetLogger logger)
        {
            var propertyInfo = logger.GetType()
                                     .GetProperty("Options", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            return propertyInfo.GetValue(logger) as Log4NetProviderOptions;
        }
    }
}