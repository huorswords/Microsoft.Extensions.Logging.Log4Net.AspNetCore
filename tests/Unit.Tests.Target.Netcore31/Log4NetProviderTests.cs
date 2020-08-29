using FluentAssertions;
using Microsoft.Extensions.Logging;
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
    }
}