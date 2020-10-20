using FluentAssertions;
using FluentAssertions.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Scope;
using Unit.Tests.Target.Netcore31.Fixtures;
using Xunit;

namespace Unit.Tests.Target.Netcore31
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
        public void WhenScopeFactoryIsNullOnProviderOptions_ThenDefaultLog4NetScopeFactoryIsUsed()
        {
            var options = new Log4NetProviderOptions
            {
                ScopeFactory = null
            };

            var sut = new Log4NetProvider(options);

            var logger = sut.CreateLogger("test") as Log4NetLogger;

            var internalOptions = GetInternalOptions(logger);
            internalOptions.Should().NotBeNull();
            internalOptions.ScopeFactory.Should().NotBeNull("Scope factory on logger's options should not be null.");
        }

        [Fact]
        public void WhenScopeFactoryIsProvidedInProviderOptions_ThenLoggerUsesProvidedScopeFactory()
        {
            var expectedFactory = new Log4NetScopeFactory(new Log4NetScopeRegistry());
            var options = new Log4NetProviderOptions
            {
                ScopeFactory = expectedFactory
            };

            var sut = new Log4NetProvider(options);
            var logger = sut.CreateLogger("test") as Log4NetLogger;

            var internalOptions = GetInternalOptions(logger);

            internalOptions.Should().NotBeNull();
            internalOptions.ScopeFactory.Should().NotBeNull("Scope factory on logger's options should not be null.")
                                                 .And.Be(expectedFactory, "Scope factory on logger does not match factory from provider options.");
        }

        private Log4NetProviderOptions GetInternalOptions(Log4NetLogger logger)
        {
            return logger.GetType()
                         .GetPropertyByName("Options")
                         .GetValue(logger) as Log4NetProviderOptions;
        }
    }
}