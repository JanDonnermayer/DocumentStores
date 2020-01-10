using System;
using System.Diagnostics;

#nullable enable

namespace DocumentStores.Primitives
{

    /// <summary>
    /// Represents the result of a performed operation, with custom return data.
    /// </summary>
    /// <usage>
    /// var result = Result{Foo}.Ok(foo1);
    /// var success = result.Try(out Foo data);
    /// </usage>
    [DebuggerStepThrough]
    public sealed class Result<TData> where TData : class
    {
        private Result(TData? data = null, Exception? exception = null)
        {
            this.data = data;
            this.exception = exception;
        }

        private readonly TData? data;
        private readonly Exception? exception;

        /// <summary>
        /// Tests the result for success.
        /// </summary>
        public bool Try() => Try(out _, out _);

        /// <summary>
        /// Tests the result for success.
        /// If the result is successful: Yields the contained <paramref name="data"/>.
        /// </summary>
        public bool Try(out TData? data) => Try(out data, out _);

        /// <summary>
        /// Tests the result for success.
        /// If the result is not successful: Yields the contained <see cref="Exception"/> (otherwise null).
        /// </summary>
        public bool Try(out Exception? ex)
        {
            ex = this.exception;
            return exception == null;
        }

        /// <summary>
        /// Tests the result for success.
        /// If the result is successful: Yields the contained <paramref name="data"/> (otherwise null).
        /// Else: Yields the contained <paramref name="exception"/> (otherwise null).
        /// </summary>
        public bool Try(out TData? data, out Exception? exception)
        {
            data = this.data;
            exception = this.exception;
            return exception == null;
        }


#nullable restore

        /// <summary>
        /// If the result is successful: Passes the contained data;
        /// else: throws an <see cref="ResultException"/> containing the underlying <see cref="Exception"/>
        /// </summary>
        public TData PassOrThrow()
        {
            if (!this.Try(out var res, out var ex)) throw new ResultException(ex);
            return res;
        }

#nullable enable

        internal static Result<TData> Ok(TData data) =>
            new Result<TData>(data ?? throw new ArgumentNullException(nameof(data)), null);

        internal static Result<Unit> Ok() =>
            new Result<Unit>(Unit.Default);

        internal static Result<TData> Error(Exception exception) =>
            new Result<TData>(exception: exception ?? throw new ArgumentNullException(nameof(exception)));

        /// <inheritdoc/>
        public static implicit operator TData(Result<TData> result) =>
            result.Try(out var data, out var ex) switch
            {
                true => data!,
                false => throw new ResultException(ex!)
            };

        /// <inheritdoc/>
        public static implicit operator Result<TData>(TData data) => Ok(data);

        /// <inheritdoc/>
        public static implicit operator Result<TData>(Exception exception) => Error(exception);
    }

}