namespace Microsoft.Extensions.Logging.Scope.Registers
{
    using System;
    using System.Collections.Generic;

    using log4net;

    public sealed class Log4NetObjectScopedRegister : Log4NetScopedRegister
    {
        public Log4NetObjectScopedRegister()
        {
            base.Type = typeof(object);
        }
        public override IEnumerable<IDisposable> AddToScope(object state)
        {
            if (state != null)
            {
                yield return LogicalThreadContext.Stacks[DefaultStackName].Push(state.ToString());
            }
        }
    }
}