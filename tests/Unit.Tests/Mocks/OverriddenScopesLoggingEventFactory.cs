using System;
using log4net.Core;
using Microsoft.Extensions.Logging;

namespace Unit.Tests.Mocks
{
    internal class OverriddenScopesLoggingEventFactory : Log4NetLoggingEventFactory
    {
        private readonly Action<LoggingEvent> _enrichWithScopesOverride;

        public OverriddenScopesLoggingEventFactory(Action<LoggingEvent> enrichWithScopesOverride)
        {
            _enrichWithScopesOverride = enrichWithScopesOverride;
        }

        protected override void EnrichWithScopes(LoggingEvent loggingEvent, IExternalScopeProvider scopeProvider)
            => _enrichWithScopesOverride(loggingEvent);
    }
}