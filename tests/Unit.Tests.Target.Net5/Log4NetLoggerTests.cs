using FluentAssertions;
using log4net.Appender;
using log4net.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Log4Net.AspNetCore.Entities;
using Microsoft.Extensions.Logging.Scope;
using Moq;
using System;
using System.IO;
using System.Linq;
using Unit.Tests.Target.Netcore31.Fixtures;
using Unit.Tests.Target.Netcore31.Models;
using Xunit;

namespace Unit.Tests.Target.Netcore31
{
    [Collection("AppenderCollection")]
    public class Log4NetLoggerTests
    {
        private readonly string _logState = "Test message";
        private readonly EventId _eventId = new EventId(5);

        public Log4NetLoggerTests(AppenderFixture context)
        {
            Context = context;
        }

        private AppenderFixture Context { get; }

        [Fact]
        public void Ctor_Should_Throw_WhenOptionsAreNull()
        {
            Assert.Throws<ArgumentNullException>(() => new Log4NetLogger(null));
        }

        [Fact]
        public void Ctor_Should_InitializeWithDefaults()
        {
            Log4NetProviderOptions options = ConfigureOptions(Log4NetFileOption.NoAppenders);

            var sut = new Log4NetLogger(options);

            sut.Should().NotBeNull();
            sut.Name.Should().Be(options.Name);
        }

        [Theory]
        [ClassData(typeof(IsEnabledClassData))]
        public void IsEnabled_GivenLogLevel_Should_ReturnExpectedValue_BasedOn_ConfigurationFile(Log4NetFileOption fileOption, LogLevel level, bool expected)
        {
            var options = ConfigureOptions(fileOption);
            var sut = new Log4NetLogger(options);
            sut.IsEnabled(level)
               .Should()
               .Be(expected);
        }

        [Theory]
        [ClassData(typeof(TestLogMethodData))]
        public void Log_Should_AddMessage_With_ExpectedLevel(LogLevel logLevel, Level eventLevel)
        {
            var options = ConfigureOptions(Log4NetFileOption.TestAppenderTrace);
            var testAppender = GetTestAppender(options);

            var sut = new Log4NetLogger(options);
            sut.Log(logLevel, _eventId, _logState, null, (message, exception) => message);

            testAppender.GetEvents()
                        .Should()
                        .NotBeEmpty()
                        .And
                        .ContainSingle();
            testAppender.GetEvents()
                        .First()
                        .Level
                        .Should()
                        .Be(eventLevel);
            testAppender.GetEvents()
                        .First()
                        .RenderedMessage
                        .Should()
                        .Contain(_logState);
        }

        [Fact]
        public void Log_Should_AddMessageAsCritical_When_LogLevelCritical_And_OverrideCriticalLevelWithIsSet()
        {
            var options = ConfigureOptions(Log4NetFileOption.TestAppenderTrace);
            options.OverrideCriticalLevelWith = "Critical";

            var testAppender = GetTestAppender(options);

            var sut = new Log4NetLogger(options);
            sut.Log(LogLevel.Critical, _eventId, _logState, null, (message, exception) => message);

            testAppender.GetEvents()
                        .Should()
                        .NotBeEmpty()
                        .And
                        .ContainSingle();
            testAppender.GetEvents()
                        .First()
                        .Level
                        .Should()
                        .Be(Level.Critical);
            testAppender.GetEvents()
                        .First().RenderedMessage
                        .Should()
                        .Contain(_logState);
        }

        [Fact]
        public void Log_Should_AddMessageAsCritical_When_LogException()
        {
            var options = ConfigureOptions(Log4NetFileOption.TestAppenderTrace);
            options.OverrideCriticalLevelWith = "Critical";

            var testAppender = GetTestAppender(options);

            var sut = new Log4NetLogger(options);
            sut.Log(LogLevel.Critical, _eventId, string.Empty, new ArgumentNullException(nameof(sut)), (message, exception) => exception.Message);

            testAppender.GetEvents()
                        .Should()
                        .NotBeEmpty()
                        .And
                        .ContainSingle();
            testAppender.GetEvents()
                        .First().Level
                        .Should()
                        .Be(Level.Critical);
            testAppender.GetEvents()
                        .First()
                        .RenderedMessage
                        .Should()
                        .Contain("sut");
        }

        [Fact]
        public void Log_Should_IgnoreMessage_When_Empty()
        {
            Log4NetProviderOptions options = ConfigureOptions(Log4NetFileOption.TestAppenderTrace);
            var testAppender = GetTestAppender(options);

            var sut = new Log4NetLogger(options);

            sut.Log(LogLevel.Trace, _eventId, string.Empty, null, (message, exception) => message);

            testAppender.GetEvents()
                        .Should()
                        .BeEmpty();
        }

        [Fact]
        public void Log_Should_Not_IgnoreMessage_When_Empty_For_Non_Null_Exception()
        {
            Log4NetProviderOptions options = ConfigureOptions(Log4NetFileOption.TestAppenderTrace);
            var testAppender = GetTestAppender(options);

            var sut = new Log4NetLogger(options);

            sut.Log(LogLevel.Trace, _eventId, string.Empty, new Exception("Something went wrong"), (message, exception) => message);

            testAppender.GetEvents()
                        .Should()
                        .NotBeEmpty();
        }

        [Fact]
        public void Log_Should_IgnoreMessage_With_LevelNone()
        {
            var options = ConfigureOptions(Log4NetFileOption.TestAppenderTrace);
            var testAppender = GetTestAppender(options);

            var sut = new Log4NetLogger(options);

            sut.Log(LogLevel.None, _eventId, _logState, null, (message, exception) => message);

            testAppender.GetEvents()
                        .Should()
                        .BeEmpty();
        }

        [Fact]
        public void Log_Should_Emit_LoggingEvents_Created_By_Custom_LoggingEventFactory()
        {
            var options = ConfigureOptions(Log4NetFileOption.TestAppenderTrace);

            var callStackBoundary = typeof(Log4NetLoggerTests);
            var exspectedRepository = log4net.LogManager.GetRepository(options.LoggerRepository);
            var exspectedLoggerName = "testLogger";
            var exspectedLoggingLevel = Level.Info;
            var exspectedLoggingMessage = "testMessage";
            var exspectedException = new InvalidOperationException("testException");

            var mockedFactory = new Mock<ILog4NetLoggingEventFactory>();
            mockedFactory
                .Setup(
                    f => f.CreateLoggingEvent(It.IsAny<MessageCandidate<string>>(), It.IsAny<log4net.Core.ILogger>(), options)
                )
                .Returns(
                new LoggingEvent(
                        callStackBoundary,
                        exspectedRepository,
                        exspectedLoggerName,
                        exspectedLoggingLevel,
                        exspectedLoggingMessage,
                        exspectedException
                     )
                );

            options.LoggingEventFactory = mockedFactory.Object;
            var testAppender = GetTestAppender(options);
            var sut = new Log4NetLogger(options);

            sut.Log(LogLevel.Debug, _eventId, _logState, null, (message, exception) => message);

            testAppender.GetEvents()
                        .Should()
                        .NotBeEmpty()
                        .And
                        .HaveCount(1);

            var loggingEvent = testAppender.GetEvents()
                                           .First();

            loggingEvent.Repository
                        .Should()
                        .Be(exspectedRepository);
            loggingEvent.LoggerName
                        .Should()
                        .Be(exspectedLoggerName);
            loggingEvent.Level
                        .Should()
                        .Be(exspectedLoggingLevel);
            loggingEvent.MessageObject
                        .Should()
                        .Be(exspectedLoggingMessage);
            loggingEvent.ExceptionObject
                        .Should()
                        .Be(exspectedException);
        }

        [Fact]
        public void Log_Should_Throw_When_FormaterIsInvalid()
        {
            var options = ConfigureOptions(Log4NetFileOption.TestAppenderTrace);
            var testAppender = GetTestAppender(options);

            var sut = new Log4NetLogger(options);

            Assert.Throws<ArgumentNullException>(() => sut.Log(LogLevel.Information, _eventId, _logState, null, null));
        }

        [Fact]
        public void Log_Should_Throw_When_LogLevelIsNotValid()
        {
            var options = ConfigureOptions(Log4NetFileOption.TestAppenderTrace);
            var testAppender = GetTestAppender(options);

            var sut = new Log4NetLogger(options);
            Assert.Throws<ArgumentOutOfRangeException>(() => sut.Log((LogLevel)52, _eventId, _logState, null, (message, exception) => message));
        }

        [Fact]
        public void BeginScope_Should_Include_CustomScopeState_In_LoggedMessage()
        {
            const string CustomScope = "CustomScope";

            var mockedFactory = new Mock<ILog4NetScopeFactory>(MockBehavior.Default);
            mockedFactory.Setup(x => x.BeginScope(It.IsAny<string>()))
                         .Returns(new Log4NetScope(CustomScope, new Log4NetScopeRegistry()));

            var options = ConfigureOptions(Log4NetFileOption.TestAppenderTrace);
            options.ScopeFactory = mockedFactory.Object;
            var testAppender = GetTestAppender(options);

            var sut = new Log4NetLogger(options);

            using (var scope = sut.BeginScope(CustomScope))
            {
                sut.Log(LogLevel.Critical, _eventId, _logState, null, (message, exception) => message);
            }

            sut.Log(LogLevel.Critical, _eventId, _logState, null, (message, exception) => message);

            // https://stackoverflow.com/questions/14438217/memoryappender-patternlayout-not-rendering
            testAppender.GetEvents()
                        .Should()
                        .NotBeEmpty()
                        .And
                        .HaveCount(2);
            testAppender.GetEvents()
                        .First()
                        .Level
                        .Should()
                        .Be(Level.Fatal);
            var textWriter = new StringWriter();
            testAppender.Layout.Format(textWriter, testAppender.GetEvents().First());
            textWriter.ToString()
                      .Should()
                      .Contain(_logState);
            textWriter.ToString()
                      .Should()
                      .Contain(CustomScope);

            textWriter.Close();
            textWriter = new StringWriter();
            testAppender.Layout.Format(textWriter, testAppender.GetEvents().Last());
            textWriter.ToString()
                      .Should()
                      .NotContain(CustomScope);
        }

        private Log4NetProviderOptions ConfigureOptions(Log4NetFileOption testAppender)
            => Context.ConfigureOptions(testAppender);

        private MemoryAppender GetTestAppender(Log4NetProviderOptions options)
            => Context.GetTestAppender(options);
    }
}