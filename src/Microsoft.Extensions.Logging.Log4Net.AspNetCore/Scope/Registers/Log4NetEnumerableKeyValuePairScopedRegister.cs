using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
#if NETCOREAPP1_1
using System.Reflection;
#endif

namespace Microsoft.Extensions.Logging.Scope.Registers
{
    public sealed class Log4NetEnumerableScopedRegister : Log4NetScopedRegister
    {
        public Log4NetEnumerableScopedRegister()
        {
            Type = typeof(IEnumerable);
        }

        public override IEnumerable<IDisposable> AddToScope(object state)
        {
            if (state is IEnumerable col)
            {
                foreach (var item in col)
                {
                    Type itemType = item.GetType();
                    if (itemType.IsAssignableFrom(typeof(KeyValuePair<string, string>)))
                    {
                        var keyValuePair = (KeyValuePair<string, string>)item;
                        yield return LogicalThreadContext.Stacks[keyValuePair.Key].Push(keyValuePair.Value);
                    }

                    if (itemType.IsAssignableFrom(typeof(KeyValuePair<string, object>)))
                    {
                        var keyValuePair = (KeyValuePair<string, object>)item;
                        yield return LogicalThreadContext.Stacks[keyValuePair.Key].Push(keyValuePair.Value?.ToString());
                    }

                    if (itemType.IsAssignableFrom(typeof(object)))
                    {
                        yield return LogicalThreadContext.Stacks[DefaultStackName].Push(item.ToString());
                    }
                }
            }
        }
    }
}