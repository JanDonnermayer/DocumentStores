using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DocumentStores.Primitives
{
    /// <summary>
    /// Provides helper methods for <see cref="IDisposable"/> implementations.
    /// </summary>
    public class DisposeHandle<TOwner> : IDisposable
        where TOwner : class
    {
        private int _disposed;
        private readonly Stack<IDisposable> _disposeStack;


        /// <summary>
        /// Initializes a new instance of the <see cref="DisposeHandle"/> class.
        /// </summary>
        /// <param name="objectName">The name of the object that employs this handle.</param>
        public DisposeHandle(params IDisposable[] disposables)
        {
             _disposeStack = new Stack<IDisposable>(disposables ?? new IDisposable[] { });
            CancellationTokenSource CTS = new CancellationTokenSource();
            this.CancellationToken = CTS.Token;
            _disposeStack.Push(new Disposable(() => CTS.Cancel()));
        }


        public bool IsDisposed =>
            _disposed == 1;

        /// <summary>
        /// Returns a token that is cancelled, as soon as this instance is disposed.
        /// </summary>
        /// <returns></returns>
        public CancellationToken CancellationToken { get; }

        /// <summary>
        /// Appends an instance of <see cref="IDisposable"/> to the dispose routine.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns> 
        /// The current instance for chaining.
        /// </returns>
        public DisposeHandle<TOwner> Append(IDisposable instance)
        {
            this.Test();
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            _disposeStack.Push(instance);
            return this;
        }

        /// <summary>
        /// Appends an action to the dispose routine.
        /// </summary>
        /// <param name="action"></param>
        /// <returns> 
        /// The current instance for chaining.
        /// </returns>
        public DisposeHandle<TOwner> Append(Action action)
        {
            this.Test();
            if (action == null) throw new ArgumentNullException(nameof(action));
            _disposeStack.Push(new Disposable(action));
            return this;
        }

        /// <summary>
        /// If disposed, throws an <see cref="ObjectDisposedException"/>
        /// </summary>
        public void Test()
        {
            if (_disposed == 1)
                throw new ObjectDisposedException(typeof(TOwner).Name);
        }

        public void Dispose()
        {
            if (System.Threading.Interlocked.Exchange(ref _disposed, 1) == 1)
                throw new ObjectDisposedException(typeof(TOwner).Name);
            while (_disposeStack.Any())
                _disposeStack.Pop().Dispose();
        }

    }
}