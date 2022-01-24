using System;

namespace Microsoft.Extensions.Logging.Log4Net.AspNetCore.Scope
{
    /// <summary>
    /// A logger scope that does not save any information and does not need to be disposed.
    /// </summary>
    internal class NullScope : IDisposable
    {
        /// <summary>
        /// The singleton instance that represent every <see cref="NullScope"/>.
        /// </summary>
        internal static NullScope Instance { get; } = new NullScope();

        /// <summary>
        /// Constructor that prevents external instantiation.
        /// </summary>
        private NullScope()
        {
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // This is a null scope so we need to dispose nothing.
        }
    }
}
