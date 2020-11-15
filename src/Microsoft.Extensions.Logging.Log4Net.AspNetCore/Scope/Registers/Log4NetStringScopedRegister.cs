using log4net;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.Logging.Scope.Registers
{
    public sealed class Log4NetStringScopedRegister : Log4NetScopedRegister
    {
        public Log4NetStringScopedRegister()
        {
            base.Type = typeof(string);
        }

        public override IEnumerable<IDisposable> AddToScope(object state)
        {
            if (state is string text)
            {
                yield return LogicalThreadContext.Stacks[DefaultStackName].Push(text);
            }
        }
    }
}