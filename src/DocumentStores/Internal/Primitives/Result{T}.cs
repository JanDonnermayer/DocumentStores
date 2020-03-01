using System;
using System.Diagnostics;

#nullable enable

namespace DocumentStores
{
    [DebuggerStepThrough]
    internal static class Result
    {
        public static IResult<TData> Ok<TData>(TData data) where TData : class =>
            new Result<TData>(data ?? throw new ArgumentNullException(nameof(data)), null);

        public static IResult<Unit> Ok() =>
            new Result<Unit>(Unit.Default);

        public static IResult<TData> Error<TData>(Exception exception) where TData : class =>
            new Result<TData>(exception: exception ?? throw new ArgumentNullException(nameof(exception)));
    }

    [DebuggerStepThrough]
    internal sealed class Result<TData> : IResult<TData> where TData : class
    {
        internal Result(TData? data = null, Exception? exception = null)
        {
            this.Data = data;
            this.Exception = exception;
            this.Success = exception is null;
        }

        public bool Success { get; }

        public Exception? Exception { get; }

        public TData? Data { get; }
    }
}