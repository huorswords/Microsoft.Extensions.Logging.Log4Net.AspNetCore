using log4net.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Log4Net.AspNetCore.Entities;
using System;

namespace Unit.Tests.Mocks
{
    internal class LoggingEventFactoryMock : ILog4NetLoggingEventFactory
    {
        private readonly LoggingEvent returnedEvent;

        public LoggingEventFactoryMock(LoggingEvent returnedEvent)
        {
            this.returnedEvent = returnedEvent ?? throw new ArgumentNullException(nameof(returnedEvent));
        }

        public LoggingEvent CreateLoggingEvent<TState>(in MessageCandidate<TState> messageCandidate, log4net.Core.ILogger logger, Log4NetProviderOptions options, IExternalScopeProvider scopeProvider)
        {
            return this.returnedEvent;
        }
    }
}