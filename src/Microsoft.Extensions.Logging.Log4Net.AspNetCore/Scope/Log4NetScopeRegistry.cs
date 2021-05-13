using Microsoft.Extensions.Logging.Scope.Registers;
using System;
using System.Collections.Generic;
using System.Linq;
#if NETCOREAPP1_1
using System.Reflection;
#endif

namespace Microsoft.Extensions.Logging.Scope
{
    public class Log4NetScopeRegistry
    {
        private readonly IDictionary<Type, Func<object, IEnumerable<IDisposable>>> registry = new Dictionary<Type, Func<object, IEnumerable<IDisposable>>>();

        public Log4NetScopeRegistry()
        {
            SetRegister(new Log4NetObjectScopedRegister());
        }

        public Func<object, IEnumerable<IDisposable>> GetRegister(Type type)
        {
            if (registry.ContainsKey(type))
            {
                return registry[type];
            }

            foreach (var item in registry.Where(x => x.Key != typeof(object)))
            {
                if (item.Key.IsAssignableFrom(type))
                {
                    return item.Value;
                }
            }

            return registry[typeof(object)];
        }

        public Log4NetScopeRegistry SetRegister(Log4NetScopedRegister property)
            => SetRegister(property.Type, property.AddToScope);

        public Log4NetScopeRegistry SetRegister(Type type, Func<object, IEnumerable<IDisposable>> register)
        {
            if (registry.ContainsKey(type))
            {
                registry[type] = register;
            }
            else
            {
                registry.Add(type, register);
            }

            return this;
        }
    }
}