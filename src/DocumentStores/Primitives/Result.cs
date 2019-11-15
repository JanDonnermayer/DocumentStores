using System;

#nullable enable

namespace DocumentStores.Primitives
{
    /// <summary>
    /// Represents the result of a performed operation.
    /// </summary>
    public class Result
    {
        private Result(Exception? exception)
        {
            this.exception = exception;
        }

        private readonly Exception? exception;


        /// <summary>
        /// Tests the result for success.
        /// </summary>
        public bool Try() => Try(out _);

        /// <summary>
        /// Tests the result for success.
        /// If the result is not successful: Yields the contained <see cref="Exception"/> (otherwise <see cref="null"/>).
        /// </summary>
        public bool Try(out Exception? ex)
        {
            ex = this.exception;
            return exception == null;
        }

#nullable restore

        /// <summary>
        /// If the result is successful: Does nothing;
        /// else: throws an <see cref="ResultException"/> containing the underlying <see cref="Exception"/>
        /// </summary>
        public void PassOrThrow()
        {
            if (!this.Try(out var ex)) throw new ResultException(ex);
        }

#nullable enable

        internal static Result Ok() =>
            new Result(exception: null);

        internal static Result Error(Exception exception) =>
            new Result(exception: exception ?? throw new ArgumentNullException(nameof(exception)));

        public static implicit operator Result(Exception ex) => Error(ex);

    }


    /// <summary>
    /// Represents the result of a performed operation, with custom return data.
    /// </summary>
    /// <usage>
    /// var result = Result<Foo>.Ok(foo1);
    /// var success = result.Try(out Foo data);
    /// </usage>
    public class Result<TData>
        where TData : class
    {
        private Result(TData? data, Exception? exception)
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
        /// If the result is successful: Yields the contained data.
        /// </summary>
        public bool Try(out TData? data) => Try(out data, out _);

        /// <summary>
        /// Tests the result for success.
        /// If the result is successful: Yields the contained data (otherwise <see cref="null"/>).
        /// Else: Yields the contained exception (otherwise <see cref="null"/>).
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

        internal static Result<TData> Error(Exception exception) =>
            new Result<TData>(null, exception ?? throw new ArgumentNullException(nameof(exception)));

        public static implicit operator Result<TData>(TData data) => Ok(data);
        public static implicit operator Result<TData>(Exception ex) => Error(ex);

    }

}