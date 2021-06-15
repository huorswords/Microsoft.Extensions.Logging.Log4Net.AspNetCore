﻿using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.Logging.Scope
{
    public class Log4NetScope : IDisposable
    {
        private bool disposedValue = false; // To detect redundant calls

        private readonly Stack<IDisposable> disposables = new Stack<IDisposable>();

        public Log4NetScope(object scope, Log4NetScopeRegistry registry)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            var type = scope.GetType();

            var register = registry.GetRegister(type);
            foreach (var disposable in register(scope))
            {
                disposables.Push(disposable);
            }
        }

        ~Log4NetScope()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    while (disposables.Count > 0)
                    {
                        var item = disposables.Pop();
                        item.Dispose();
                    }

                    disposables.Clear();
                }

                disposedValue = true;
            }
        }
    }
}