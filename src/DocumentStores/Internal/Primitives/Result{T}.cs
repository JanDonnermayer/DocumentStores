using System;
using System.Diagnostics;

#nullable enable

namespace DocumentStores
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
            this.Data = data;
            this.Exception = exception;
        }

        /// <summary>
        /// Whether this resut is successfull.
        /// </summary>
        public bool Success => Exception is null;

        /// <summary>
        /// The exception contained, when the result is not successfull.
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// The data contained, when the result is successfull.
        /// </summary>
        public TData? Data { get; }

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
        /// If the result is not successful: Yields the contained <see cref="System.Exception"/> (otherwise null).
        /// </summary>
        public bool Try(out Exception? ex)
        {
            ex = this.Exception;
            return Exception == null;
        }

        /// <summary>
        /// Tests the result for success.
        /// If the result is successful: Yields the contained <paramref name="data"/> (otherwise null).
        /// Else: Yields the contained <paramref name="exception"/> (otherwise null).
        /// </summary>
        public bool Try(out TData? data, out Exception? exception)
        {
            data = this.Data;
            exception = this.Exception;
            return exception == null;
        }

        /// <summary>
        /// Deconstructs this instance into contained data.
        /// </summary>
        public void Deconstruct(out TData? data, out Exception? exception)
        {
            data = this.Data;
            exception = this.Exception;
        }

        /// <summary>
        /// If the result is successful: Passes the contained data;
        /// else: throws an <see cref="ResultException"/> containing the underlying <see cref="System.Exception"/>
        /// </summary>
        public TData PassOrThrow()
        {
            if (!this.Try(out var res, out var ex))
                throw new ResultException(ex!);
            return res!;
        }

        internal static Result<TData> Ok(TData data) =>
            new Result<TData>(data ?? throw new ArgumentNullException(nameof(data)), null);

        internal static Result<Unit> Ok() =>
            new Result<Unit>(Unit.Default);

        internal static Result<TData> Error(Exception exception) =>
            new Result<TData>(exception: exception ?? throw new ArgumentNullException(nameof(exception)));

#pragma warning disable CA2225 // Provide alternatives for op_implicit

        /// <inheritdoc/>
        public static implicit operator TData?(Result<TData> result) =>
            result switch
            {
                null => null,
                Result<TData> res => res switch
                {
                    (TData data, _) => data!,
                    _ => null
                }
            };

        /// <inheritdoc/>
        public static implicit operator Result<TData>(TData data) => Ok(data);

        /// <inheritdoc/>
        public static implicit operator Result<TData>(Exception exception) => Error(exception);

#pragma warning restore
    }
}