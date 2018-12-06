using System;
using System.Threading;

namespace Tavisca.Platform.Common.Context
{
    public sealed class AmbientContextScope : IDisposable
    {
        private bool _disposed;
        private readonly AmbientContextBase _originalContext;
        private readonly AmbientContextBase _currentContext;
        private readonly AmbientContextScope _originalContextScope;

        private static readonly AsyncLocal<AmbientContextScope> CurrentContextScope = new AsyncLocal<AmbientContextScope>();

        public static AmbientContextScope Current => CurrentContextScope.Value;

        public AmbientContextScope(AmbientContextBase context)
        {
            _originalContext = AmbientContextBase.Current;
            _originalContextScope = CurrentContextScope.Value;
            CurrentContextScope.Value = this;
            AmbientContextBase.Current = context;
            _currentContext = context;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            if (CurrentContextScope.Value != this)
                throw new InvalidOperationException("There was corruption of data in the context, the current context scope data does not match. Possibility of thread interleaving.");
            if (AmbientContextBase.Current != this._currentContext)
                throw new InvalidOperationException("There was corruption of data in the context, the current context data does not match. Possibility of thread interleaving.");

            CurrentContextScope.Value = this._originalContextScope;
            AmbientContextBase.Current = this._originalContext;
        }
    }
}
