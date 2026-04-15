using FluentAssertions;
using log4net.Appender;
using log4net.Core;
using Microsoft.Extensions.Logging;
#if NET6_0
using Microsoft.Extensions.Logging.Log4Net.AspNetCore.Entities;
#endif
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unit.Tests.Fixtures;
using Unit.Tests.Mocks;
using Unit.Tests.Models;
using Xunit;

namespace Unit.Tests
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
            Assert.Throws<ArgumentNullException>(() => new Log4NetLogger(null, new LoggerExternalScopeProvider()));
        }

        [Fact]
        public void Ctor_Should_Throw_WhenExternalScopeProviderIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new Log4NetLogger(new Log4NetProviderOptions(), null));
        }

        [Fact]
        public void Ctor_Should_InitializeWithDefaults()
        {
            Log4NetProviderOptions options = ConfigureOptions(Log4NetFileOption.NoAppenders);
            LoggerExternalScopeProvider scopeProvider = new LoggerExternalScopeProvider();

            var sut = new Log4NetLogger(options, scopeProvider);

            sut.Should().NotBeNull();
            sut.Name.Should().Be(options.Name);
        }

        [Theory]
        [ClassData(typeof(IsEnabledClassData))]
        public void IsEnabled_GivenLogLevel_Should_ReturnExpectedValue_BasedOn_ConfigurationFile(Log4NetFileOption fileOption, LogLevel level, bool expected)
        {
            var options = ConfigureOptions(fileOption);
            LoggerExternalScopeProvider scopeProvider = new LoggerExternalScopeProvider();

            var sut = new Log4NetLogger(options, scopeProvider);
            sut.IsEnabled(level)
               .Should()
               .Be(expected);
        }

        [Fact]
        public void Log_Should_EnsureEventIdProperty()
        {
            var options = ConfigureOptions(Log4NetFileOption.TestAppenderTrace);

            LoggerExternalScopeProvider scopeProvider = new LoggerExternalScopeProvider();
            var testAppender = GetTestAppender(options);

            var sut = new Log4NetLogger(options, scopeProvider);
            sut.Log(LogLevel.Information, _eventId, _logState, null, (message, exception) => message);

            testAppender.GetEvents()
                        .First()
                        .GetProperties()["eventId"]
                        .Should()
                        .Be(_eventId);
        }

        [Theory]
        [ClassData(typeof(TestLogMethodData))]
        public void Log_Should_AddMessage_With_ExpectedLevel(LogLevel logLevel, Level eventLevel)
        {
            var options = ConfigureOptions(Log4NetFileOption.TestAppenderTrace);
            LoggerExternalScopeProvider scopeProvider = new LoggerExternalScopeProvider();
            var testAppender = GetTestAppender(options);

            var sut = new Log4NetLogger(options, scopeProvider);
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

            LoggerExternalScopeProvider scopeProvider = new LoggerExternalScopeProvider();
            var testAppender = GetTestAppender(options);

            var sut = new Log4NetLogger(options, scopeProvider);
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

            LoggerExternalScopeProvider scopeProvider = new LoggerExternalScopeProvider();
            var testAppender = GetTestAppender(options);

            var sut = new Log4NetLogger(options, scopeProvider);
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
            LoggerExternalScopeProvider scopeProvider = new LoggerExternalScopeProvider();

            var sut = new Log4NetLogger(options, scopeProvider);

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
            LoggerExternalScopeProvider scopeProvider = new LoggerExternalScopeProvider();

            var sut = new Log4NetLogger(options, scopeProvider);

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
            LoggerExternalScopeProvider scopeProvider = new LoggerExternalScopeProvider();

            var sut = new Log4NetLogger(options, scopeProvider);

            sut.Log(LogLevel.None, _eventId, _logState, null, (message, exception) => message);

            testAppender.GetEvents()
                        .Should()
                        .BeEmpty();
        }

        [Fact]
        public void Log_Should_Emit_LoggingEvents_Created_By_Custom_LoggingEventFactory()
        {
            var options = ConfigureOptions(Log4NetFileOption.TestAppenderTrace);
            LoggerExternalScopeProvider scopeProvider = new LoggerExternalScopeProvider();

            var callStackBoundary = typeof(Log4NetLoggerTests);
            var expectedRepository = log4net.LogManager.GetRepository(options.LoggerRepository);
            var expectedLoggerName = "testLogger";
            var expectedLoggingLevel = Level.Info;
            var expectedLoggingMessage = "testMessage";
            var expectedException = new InvalidOperationException("testException");
            var expectedCustomPropertyValue = ConsoleColor.Red;

            var expectedLoggingEvent = new LoggingEvent(
                callStackBoundary,
                expectedRepository,
                expectedLoggerName,
                expectedLoggingLevel,
                expectedLoggingMessage,
                expectedException
            );

            expectedLoggingEvent.Properties["OutputColor"] = expectedCustomPropertyValue;

            // We can't use Moq until .NET 6 here because of a bug in the .NET runtime with generic in parameters.
            // https://github.com/moq/moq4/issues/1148
#if NET6_0
            var mockedFactory = new Mock<ILog4NetLoggingEventFactory>(MockBehavior.Strict);
            mockedFactory
                .Setup(
                    f => f.CreateLoggingEvent(in It.Ref<MessageCandidate<string>>.IsAny, It.IsAny<log4net.Core.ILogger>(), options, scopeProvider)
                )
                .Returns(
                    expectedLoggingEvent
                );
            options.LoggingEventFactory = mockedFactory.Object;
#else
            options.LoggingEventFactory = new LoggingEventFactoryMock(returnedEvent: expectedLoggingEvent);
#endif

            var testAppender = GetTestAppender(options);
            var sut = new Log4NetLogger(options, scopeProvider);

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
                        .Be(expectedRepository);
            loggingEvent.LoggerName
                        .Should()
                        .Be(expectedLoggerName);
            loggingEvent.Level
                        .Should()
                        .Be(expectedLoggingLevel);
            loggingEvent.MessageObject
                        .Should()
                        .Be(expectedLoggingMessage);
            loggingEvent.ExceptionObject
                        .Should()
                        .Be(expectedException);
            loggingEvent.GetProperties()["OutputColor"]
                        .Should()
                        .Be(expectedCustomPropertyValue);
        }

        [Fact]
        public void Log_Should_Emit_Custom_Scope_Operations_Created_By_Overriden_Enrich_Scopes_Method()
        {
            var options = ConfigureOptions(Log4NetFileOption.TestAppenderTrace);
            LoggerExternalScopeProvider scopeProvider = new LoggerExternalScopeProvider();

            var expectedRepository = log4net.LogManager.GetRepository(options.LoggerRepository);
            var customPropertyValue = "customStuff";
            var expectedCustomPropertyValue = (4, 3f);

            options.LoggingEventFactory = new OverriddenScopesLoggingEventFactory(
                (@event) => @event.Properties[customPropertyValue] = expectedCustomPropertyValue);

            var testAppender = GetTestAppender(options);
            var sut = new Log4NetLogger(options, scopeProvider);

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
                        .Be(expectedRepository);
            loggingEvent.Level
                        .Should()
                        .Be(Level.Debug);
            loggingEvent.MessageObject
                        .Should()
                        .Be(_logState);
            loggingEvent.GetProperties()[customPropertyValue]
                        .Should()
                        .Be(expectedCustomPropertyValue);
        }

        [Fact]
        public void Log_Should_Emit_Custom_Scope_Operations_Created_By_Overriden_Enrich_Properties_Method()
        {
            var options = ConfigureOptions(Log4NetFileOption.TestAppenderTrace);
            LoggerExternalScopeProvider scopeProvider = new LoggerExternalScopeProvider();

            var expectedRepository = log4net.LogManager.GetRepository(options.LoggerRepository);
            var customPropertyValue = "customStuff";
            var expectedCustomPropertyValue = (4, 3f);

            options.LoggingEventFactory = new OverriddenPropertiesLoggingEventFactory(
                (@event) => @event.Properties[customPropertyValue] = expectedCustomPropertyValue);

            var testAppender = GetTestAppender(options);
            var sut = new Log4NetLogger(options, scopeProvider);

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
                        .Be(expectedRepository);
            loggingEvent.Level
                        .Should()
                        .Be(Level.Debug);
            loggingEvent.MessageObject
                        .Should()
                        .Be(_logState);
            loggingEvent.GetProperties()[customPropertyValue]
                        .Should()
                        .Be(expectedCustomPropertyValue);
        }

        [Fact]
        public void Log_Should_Emit_At_LogLevels_Translate_By_LogLevelTranslator()
        {
            var options = ConfigureOptions(Log4NetFileOption.TestAppenderTrace);
            LoggerExternalScopeProvider scopeProvider = new LoggerExternalScopeProvider();

            var expectedWarningLevel = new Level(100000, "Custom Level");
            var expectedErrorLevel = Level.Emergency;

            var mockedTranslator = new Mock<ILog4NetLogLevelTranslator>(MockBehavior.Strict);
            mockedTranslator
                .Setup(
                    f => f.TranslateLogLevel(LogLevel.Warning, options)
                )
                .Returns(expectedWarningLevel);

            mockedTranslator.Setup(
                    f => f.TranslateLogLevel(LogLevel.Error, options)
                )
                .Returns(expectedErrorLevel);


            options.LogLevelTranslator = mockedTranslator.Object;
            var testAppender = GetTestAppender(options);
            var sut = new Log4NetLogger(options, scopeProvider);

            sut.Log(LogLevel.Warning, _eventId, _logState, null, (message, exception) => message);
            sut.Log(LogLevel.Error, _eventId, _logState, null, (message, exception) => message);

            testAppender.GetEvents()
                        .Should()
                        .NotBeEmpty()
                        .And
                        .HaveCount(2);

            testAppender.GetEvents()
                        .First()
                        .Level
                        .Should()
                        .Be(expectedWarningLevel);

            testAppender.GetEvents()
                        .ElementAt(1)
                        .Level
                        .Should()
                        .Be(expectedErrorLevel);
        }

        [Fact]
        public void Log_Should_Throw_When_FormaterIsInvalid()
        {
            var options = ConfigureOptions(Log4NetFileOption.TestAppenderTrace);
            var testAppender = GetTestAppender(options);
            LoggerExternalScopeProvider scopeProvider = new LoggerExternalScopeProvider();

            var sut = new Log4NetLogger(options, scopeProvider);

            Assert.Throws<ArgumentNullException>(() => sut.Log(LogLevel.Information, _eventId, _logState, null, null));
        }

        [Fact]
        public void Log_Should_Throw_When_LogLevelIsNotValid()
        {
            var options = ConfigureOptions(Log4NetFileOption.TestAppenderTrace);
            var testAppender = GetTestAppender(options);
            LoggerExternalScopeProvider scopeProvider = new LoggerExternalScopeProvider();

            var sut = new Log4NetLogger(options, scopeProvider);
            Assert.Throws<ArgumentOutOfRangeException>(() => sut.Log((LogLevel)52, _eventId, _logState, null, (message, exception) => message));
        }

        [Fact]
        public void BeginScope_Should_Include_CustomScopeState_In_LoggedMessage()
        {
            const string CustomScope = "CustomScope";

            var options = ConfigureOptions(Log4NetFileOption.TestAppenderTrace);
            var testAppender = GetTestAppender(options);
            LoggerExternalScopeProvider scopeProvider = new LoggerExternalScopeProvider();

            var sut = new Log4NetLogger(options, scopeProvider);

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

        [Fact]
        public void BeginScope_Should_Concatenate_SimpleStringCustomScopes_In_LoggedMessage()
        {
            const string CustomScope1 = "CustomScope1";
            const string CustomScope2 = "CustomScope2";

            var options = ConfigureOptions(Log4NetFileOption.TestAppenderTrace);
            var testAppender = GetTestAppender(options);
            LoggerExternalScopeProvider scopeProvider = new LoggerExternalScopeProvider();

            var sut = new Log4NetLogger(options, scopeProvider);

            using (var scope1 = sut.BeginScope(CustomScope1))
            {
                using (var scope2 = sut.BeginScope(CustomScope2))
                {
                    sut.Log(LogLevel.Critical, _eventId, _logState, null, (message, exception) => message);
                }
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
                .Contain(CustomScope1 + " " + CustomScope2);

            textWriter.Close();
            textWriter = new StringWriter();
            testAppender.Layout.Format(textWriter, testAppender.GetEvents().Last());
            textWriter.ToString()
                .Should()
                .NotContain(CustomScope1)
                .And
                .NotContain(CustomScope2);
        }

        [Fact]
        public void BeginScope_Should_Concatenate_SimpleObjectCustomScopes_In_LoggedMessage()
        {
            object CustomScope1 = 1.1;
            object CustomScope2 = 1.2;

            var options = ConfigureOptions(Log4NetFileOption.TestAppenderTrace);
            var testAppender = GetTestAppender(options);
            LoggerExternalScopeProvider scopeProvider = new LoggerExternalScopeProvider();

            var sut = new Log4NetLogger(options, scopeProvider);

            using (var scope1 = sut.BeginScope(CustomScope1))
            {
                using (var scope2 = sut.BeginScope(CustomScope2))
                {
                    sut.Log(LogLevel.Critical, _eventId, _logState, null, (message, exception) => message);
                }
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
                .Contain("1.1 1.2");

            textWriter.Close();
            textWriter = new StringWriter();
            testAppender.Layout.Format(textWriter, testAppender.GetEvents().Last());
            textWriter.ToString()
                .Should()
                .NotContain("1.1")
                .And
                .NotContain("1.2");
        }

        [Fact]
        public void BeginScope_Should_Concatenate_StringKeyValueCustomScopes_In_LoggedMessage()
        {
            IEnumerable<KeyValuePair<string, string>> CustomScope1 = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("SpanId", "CustomScope1")
            };
            IEnumerable<KeyValuePair<string, string>> CustomScope2 = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("SpanId", "CustomScope2")
            };

            var options = ConfigureOptions(Log4NetFileOption.TestAppenderWarn);
            var testAppender = GetTestAppender(options);
            LoggerExternalScopeProvider scopeProvider = new LoggerExternalScopeProvider();

            var sut = new Log4NetLogger(options, scopeProvider);

            using (var scope1 = sut.BeginScope(CustomScope1))
            {
                using (var scope2 = sut.BeginScope(CustomScope2))
                {
                    sut.Log(LogLevel.Critical, _eventId, _logState, null, (message, exception) => message);
                }
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
                .Contain("CustomScope1 CustomScope2");

            textWriter.Close();
            textWriter = new StringWriter();
            testAppender.Layout.Format(textWriter, testAppender.GetEvents().Last());
            textWriter.ToString()
                .Should()
                .NotContain("CustomScope1")
                .And
                .NotContain("CustomScope2");
        }

        [Fact]
        public void BeginScope_Should_Concatenate_ObjectKeyValueCustomScopes_In_LoggedMessage()
        {
            IEnumerable<KeyValuePair<string, object>> CustomScope1 = new KeyValuePair<string, object>[]
            {
                new KeyValuePair<string, object>("SpanId", 1.1)
            };
            IEnumerable<KeyValuePair<string, object>> CustomScope2 = new KeyValuePair<string, object>[]
            {
                new KeyValuePair<string, object>("SpanId", 2.1)
            };

            var options = ConfigureOptions(Log4NetFileOption.TestAppenderWarn);
            var testAppender = GetTestAppender(options);
            LoggerExternalScopeProvider scopeProvider = new LoggerExternalScopeProvider();

            var sut = new Log4NetLogger(options, scopeProvider);

            using (var scope1 = sut.BeginScope(CustomScope1))
            {
                using (var scope2 = sut.BeginScope(CustomScope2))
                {
                    sut.Log(LogLevel.Critical, _eventId, _logState, null, (message, exception) => message);
                }
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
                .Contain("1.1 2.1");

            textWriter.Close();
            textWriter = new StringWriter();
            testAppender.Layout.Format(textWriter, testAppender.GetEvents().Last());
            textWriter.ToString()
                .Should()
                .NotContain("1.1")
                .And
                .NotContain("2.1");
        }

        [Fact]
        public void BeginScope_Should_Concatenate_ValueTupleCustomScopes_In_LoggedMessage()
        {
            var options = ConfigureOptions(Log4NetFileOption.TestAppenderWarn);
            var testAppender = GetTestAppender(options);
            LoggerExternalScopeProvider scopeProvider = new LoggerExternalScopeProvider();

            var sut = new Log4NetLogger(options, scopeProvider);

            using (sut.BeginScope(("SpanId", (string)"text")))
            using (sut.BeginScope(("SpanId", (int)1)))
            using (sut.BeginScope(("SpanId", (long)2)))
            using (sut.BeginScope(("SpanId", (short)3)))
            using (sut.BeginScope(("SpanId", (decimal)4.1)))
            using (sut.BeginScope(("SpanId", (double)4.2)))
            using (sut.BeginScope(("SpanId", (float)4.3)))
            using (sut.BeginScope(("SpanId", (uint)5)))
            using (sut.BeginScope(("SpanId", (ulong)6)))
            using (sut.BeginScope(("SpanId", (ushort)7)))
            using (sut.BeginScope(("SpanId", (byte)8)))
            using (sut.BeginScope(("SpanId", (sbyte)9)))
            using (sut.BeginScope(("SpanId", (object)"object")))
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
                .Contain("text 1 2 3 4.1 4.2 4.3 5 6 7 8 9 object");

            textWriter.Close();
            textWriter = new StringWriter();
            testAppender.Layout.Format(textWriter, testAppender.GetEvents().Last());
            textWriter.ToString()
                .Should()
                .NotContain("text")
                .And.NotContain("1")
                .And.NotContain("2")
                .And.NotContain("3")
                .And.NotContain("4.1")
                .And.NotContain("4.2")
                .And.NotContain("4.3")
                .And.NotContain("5")
                .And.NotContain("6")
                .And.NotContain("7")
                .And.NotContain("8")
                .And.NotContain("9")
                .And.NotContain("object");
        }

        [Fact]
        public void LogMessage_Should_Include_ScopesProvidedExternally_In_LoggedMessage()
        {
            var options = ConfigureOptions(Log4NetFileOption.TestAppenderWarn);
            var testAppender = GetTestAppender(options);
            var scopeProviderMock = new Mock<IExternalScopeProvider>(MockBehavior.Strict);

            // The default external scope provider will add something like this starting with .NET 5 from the outside.
            // So this makes sure it works.
            // See: https://github.com/dotnet/runtime/blob/2d6cc77b9ea17c18e9bc6d6197d460b50c27e792/src/libraries/Microsoft.Extensions.Logging/src/LoggerFactoryScopeProvider.cs
            scopeProviderMock.Setup(
                s => s.ForEachScope(It.IsAny<Action<object, LoggingEvent>>(), It.IsAny<LoggingEvent>())
            ).Callback<Action<object, LoggingEvent>, LoggingEvent>((scopeCallback, loggingEvent) =>
            {
                scopeCallback(new Dictionary<string, string>
                {
                    ["SpanId"] = "13371337",
                }, loggingEvent);
                scopeCallback(new Dictionary<string, string>
                {
                    ["TraceId"] = "420420420420420",
                }, loggingEvent);
            });

            var sut = new Log4NetLogger(options, scopeProviderMock.Object);

            sut.Log(LogLevel.Critical, _eventId, _logState, null, (message, exception) => message);

            // https://stackoverflow.com/questions/14438217/memoryappender-patternlayout-not-rendering
            testAppender.GetEvents()
                        .Should()
                        .NotBeEmpty()
                        .And
                        .HaveCount(1);
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
                      .StartWith("13371337")
                      .And
                      .EndWith("420420420420420" + Environment.NewLine);

            textWriter.Close();
        }

        private Log4NetProviderOptions ConfigureOptions(Log4NetFileOption testAppender)
            => Context.ConfigureOptions(testAppender);

        private MemoryAppender GetTestAppender(Log4NetProviderOptions options)
            => Context.GetTestAppender(options);
    }
}