namespace Microsoft.Extensions.Logging.Scope.Registers
{
    using System;
    using System.Collections.Generic;

    public abstract class Log4NetScopedRegister
    {
        protected const string DefaultStackName = "scope";

        public virtual Type Type { get; protected set; }

        public abstract IEnumerable<IDisposable> AddToScope(object state);
    }
}