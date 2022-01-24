using System;

namespace Microsoft.Extensions.Logging.Log4Net.AspNetCore.Scope
{
    /// <summary>
    /// A <see cref="IExternalScopeProvider"/> that will not save nor return scopes.
    /// </summary>
    internal class NullScopeProvider : IExternalScopeProvider
    {
        /// <summary>
        /// The singleton instance that represents every <see cref="NullScopeProvider"/>.
        /// </summary>
        internal static NullScopeProvider Instance { get; } = new NullScopeProvider();

        /// <summary>
        /// Constructor that prevents external instantiation.
        /// </summary>
        private NullScopeProvider()
        {
        }

        /// <inheritdoc/>
        public void ForEachScope<TState>(Action<object, TState> callback, TState state)
        {
            // All scopes are null scopes so do nothing.
        }

        /// <inheritdoc/>
        public IDisposable Push(object state)
        {
            return NullScope.Instance;
        }
    }
}
