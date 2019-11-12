using System;

#nullable enable

namespace DocumentStores.Primitives
{
    /// <summary>
    /// Represents the result of a performed operation.
    /// </summary>
    public class Result
    {
        private Result(Exception? exception) =>
            this.exception = exception;

        private readonly Exception? exception;


        public bool Try() => Try(out _);

        public bool Try(out Exception? ex)
        {
            ex = this.exception;
            return exception == null;
        }

        public static Result Ok() => new Result(exception : null);

        public static Result Error(Exception exception) =>
            new Result(exception ?? throw new ArgumentNullException(nameof(exception)));

        public static implicit operator Result(Exception ex) => Error(ex);

#nullable restore //suppress exception might be null
        public static implicit operator bool(Result result) =>
            result.exception == null ? true : throw new ResultException(result.exception);

#nullable enable
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


        public bool Try() => Try(out _, out _);

        public bool Try(out TData? data) => Try(out data, out _);

        public bool Try(out Exception? exception) => Try(out _, out exception);

        public bool Try(out TData? data, out Exception? exception)
        {
            data = this.data;
            exception = this.exception;
            return exception == null;
        }


        public static Result<TData> Ok(TData data) =>
            new Result<TData>(data ?? throw new ArgumentNullException(nameof(data)), null);

        public static Result<TData> Error(Exception exception) =>
            new Result<TData>(null, exception ?? throw new ArgumentNullException(nameof(exception)));

        public static implicit operator Result<TData>(TData data) => Ok(data);

        public static implicit operator Result<TData>(Exception ex) => Error(ex);

#nullable restore //suppress exception might be null
        public static implicit operator TData(Result<TData> result) =>
            result.data ?? throw new ResultException(result.exception);

#nullable enable

    }

}