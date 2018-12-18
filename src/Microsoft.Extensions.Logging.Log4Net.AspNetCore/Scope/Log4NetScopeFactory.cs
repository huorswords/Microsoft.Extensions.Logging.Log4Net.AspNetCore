namespace Microsoft.Extensions.Logging.Scope
{
    using Microsoft.Extensions.Logging.Scope.Registers;
    using System;

    public class Log4NetScopeFactory
    {
        private readonly Log4NetScopeRegistry registry;

        public Log4NetScopeFactory(Log4NetScopeRegistry registry)
        {
            this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
            registry.SetRegister(new Log4NetStringScopedRegister())
                    .SetRegister(new Log4NetEnumerableScopedRegister());
        }

        public Log4NetScope BeginScope<TScope>(TScope scope)
            => new Log4NetScope(scope, registry);
    }
}