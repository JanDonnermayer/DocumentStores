using System;
using System.Diagnostics;
using System.Threading;

namespace DocumentStores.Primitives
{
    /// <summary>
    /// An <see cref="IDisposable"/> implementation, that executes a specified <see cref="Action"/> on disposal
    /// </summary>
    /// <history>
    /// [25-10-2018] - Donnermayer - Created
    /// </history>
    [DebuggerStepThrough]
    public class Disposable : IDisposable
    {
        private int _disposed;
        private readonly Action _disposeAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="Disposable"/> class,
        /// that performs no action on disposal.
        /// </summary>
        public Disposable() : this(() => { }) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Disposable"/> class,
        /// that performs the specified <paramref name="disposeAction"/> on disposal.
        /// </summary>
        /// <param name="disposeAction"></param>
        public Disposable(Action disposeAction) =>
            _disposeAction = disposeAction ?? throw new ArgumentNullException(nameof(disposeAction));
               
        /// <summary>
        /// Initializes a new instance of the <see cref="Disposable"/> class,
        /// that performs the specified <paramref name="disposeAction"/> on disposal.
        /// </summary>
        /// <param name="disposeAction"></param>
        public static Disposable Create(Action disposeAction) =>
            new Disposable(disposeAction);

        /// <summary>
        /// Returns an IDisposable,
        /// that performs no action on disposal.
        /// </summary>
        public static Disposable Empty => new Disposable();

        /// <summary/>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
                throw new ObjectDisposedException(this.GetType().Name);
            _disposeAction.Invoke();
        }
    }
}