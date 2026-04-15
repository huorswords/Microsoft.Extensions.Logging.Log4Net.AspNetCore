using System;
using log4net.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Log4Net.AspNetCore.Entities;

namespace Unit.Tests.Mocks
{
    internal class OverriddenPropertiesLoggingEventFactory : Log4NetLoggingEventFactory
    {
        private readonly Action<LoggingEvent> _enrichPropertiesOverride;

        public OverriddenPropertiesLoggingEventFactory(Action<LoggingEvent> enrichPropertiesOverride)
        {
            _enrichPropertiesOverride = enrichPropertiesOverride;
        }

        protected override void EnrichProperties<TState>(LoggingEvent loggingEvent, in MessageCandidate<TState> messageCandidate)
           => _enrichPropertiesOverride(loggingEvent);
    }
}