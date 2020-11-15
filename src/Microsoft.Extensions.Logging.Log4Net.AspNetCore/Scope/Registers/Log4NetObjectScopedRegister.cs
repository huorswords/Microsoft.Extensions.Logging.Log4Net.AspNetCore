using log4net;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.Logging.Scope.Registers
{
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