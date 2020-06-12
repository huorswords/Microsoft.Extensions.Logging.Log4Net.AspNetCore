using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Linq;
using Unit.Tests.Target.Netcore31.Fixtures;
using Unit.Tests.Target.Netcore31.Models;
using Xunit;

namespace Unit.Tests.Target.Netcore31
{
    [Collection("AppenderCollection")]
    public class Log4NetExtensionsTests
    {
        public Log4NetExtensionsTests(AppenderFixture context)
        {
            Context = context;
        }

        private AppenderFixture Context { get; }

        [Fact]
        public void Should_AddLog4Net_With_Options()
        {
            var options = Context.ConfigureOptions(Log4NetFileOption.TestAppenderTrace);
            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging(builder =>
                {
                    builder.SetMinimumLevel(LogLevel.Information);
                    builder.AddLog4Net(options)
                           .AddDebug();
                }).Build();

            var sut = host.Services.GetRequiredService<ILogger<Log4NetExtensionsTests>>();

            sut.LogInformation("{current}", "value");

            var testAppender = Context.GetTestAppender(options);
            log4net.Core.LoggingEvent[] events = testAppender.GetEvents();
            events.Should().ContainSingle();
            events.First().RenderedMessage.Should()
                                          .Contain("value");
        }

        [Fact]
        public void Should_AddLog4Net_Without_Options()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging(builder =>
                {
                    builder.SetMinimumLevel(LogLevel.Information);
                    builder.AddLog4Net()
                           .AddDebug();
                }).Build();

            var sut = host.Services.GetRequiredService<ILogger<Log4NetExtensionsTests>>();

            sut.LogInformation("{current}", "value");

            var testAppender = Context.GetTestAppender();
            log4net.Core.LoggingEvent[] events = testAppender.GetEvents();
            events.Should().ContainSingle();
            events.First().RenderedMessage.Should()
                                          .Contain("value");
        }
    }
}