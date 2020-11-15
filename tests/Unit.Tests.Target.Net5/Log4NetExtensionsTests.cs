using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
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
        public void AddLog4Net_When_UsedOverILoggingBuilder_And_PassingOptions_Then_Call_ILoggerFactory_AddProvider()
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
        public void AddLog4Net_When_UsedOverILoggingBuilder_And_PassingFilename_Then_Call_ILoggerFactory_AddProvider()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging(builder =>
                {
                    builder.SetMinimumLevel(LogLevel.Information);
                    builder.AddLog4Net(Context.GetLog4netFilePath(Log4NetFileOption.TestAppenderTrace))
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

        [Fact]
        public void AddLog4Net_When_UsedOverILoggingBuilder_And_PassingFilename_And_Watch_Then_Call_ILoggerFactory_AddProvider()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging(builder =>
                {
                    builder.SetMinimumLevel(LogLevel.Information);
                    builder.AddLog4Net(Context.GetLog4netFilePath(Log4NetFileOption.TestAppenderTrace), true)
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

        [Fact]
        public void AddLog4Net_When_UsedOverILoggingBuilder_Then_Call_ILoggerFactory_AddProvider()
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

        [Fact]
        public void AddLog4Net_When_UsedOverILoggerFactory_Then_Call_ILoggerFactory_AddProvider()
        {
            Mock<ILoggerFactory> loggerFactoryMock = new Mock<ILoggerFactory>();

            var sut = loggerFactoryMock.Object.AddLog4Net();

            loggerFactoryMock.Verify(x => x.AddProvider(It.IsAny<Log4NetProvider>()), Times.Once);
        }

        [Fact]
        public void AddLog4Net_When_UsedOverILoggerFactory_And_Passing_Filename_Then_Call_ILoggerFactory_AddProvider()
        {
            Mock<ILoggerFactory> loggerFactoryMock = new Mock<ILoggerFactory>();

            var sut = loggerFactoryMock.Object.AddLog4Net("./Resources/log4net.All.config");

            loggerFactoryMock.Verify(x => x.AddProvider(It.IsAny<Log4NetProvider>()), Times.Once);
        }

        [Fact]
        public void AddLog4Net_When_UsedOverILoggerFactory_And_Passing_Filename_And_Watch_Then_Call_ILoggerFactory_AddProvider()
        {
            Mock<ILoggerFactory> loggerFactoryMock = new Mock<ILoggerFactory>();

            var sut = loggerFactoryMock.Object.AddLog4Net("./Resources/log4net.All.config", true);

            loggerFactoryMock.Verify(x => x.AddProvider(It.IsAny<Log4NetProvider>()), Times.Once);
        }

        [Fact]
        public void AddLog4Net_When_UsedOverILoggerFactory_And_PassingProviderOptions_Then_Call_ILoggerFactory_AddProvider()
        {
            Mock<ILoggerFactory> loggerFactoryMock = new Mock<ILoggerFactory>();

            var sut = loggerFactoryMock.Object.AddLog4Net(new Log4NetProviderOptions());

            loggerFactoryMock.Verify(x => x.AddProvider(It.IsAny<Log4NetProvider>()), Times.Once);
        }
    }
}