namespace Microsoft.Extensions.Logging.Scope
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using log4net;
    using Microsoft.Extensions.Logging.Scope.Registers;

    public class Log4NetScopeRegistry
    {
        private readonly IDictionary<Type, Func<object, IEnumerable<IDisposable>>> registry = new Dictionary<Type, Func<object, IEnumerable<IDisposable>>>();

        public Log4NetScopeRegistry()
        {
            this.SetRegister(new Log4NetObjectScopedRegister());
        }

        public Func<object, IEnumerable<IDisposable>> GetRegister(Type type)
        {
            if (this.registry.ContainsKey(type))
            {
                return this.registry[type];
            }

            foreach (var item in this.registry.Where(x => x.Key != typeof(object)))
            {
                if (item.Key.IsAssignableFrom(type))
                {
                    return item.Value;
                }
            }

            return this.registry[typeof(object)];
        }

        public Log4NetScopeRegistry SetRegister(Log4NetScopedRegister property) 
            => this.SetRegister(property.Type, property.AddToScope);

        public Log4NetScopeRegistry SetRegister(Type type, Func<object, IEnumerable<IDisposable>> register)
        {
            if (this.registry.ContainsKey(type))
            {
                this.registry[type] = register;
            }
            else
            {
                this.registry.Add(type, register);
            }

            return this;
        }
    }
}