using System.Threading.Tasks;
using System;

namespace DocumentStores
{
    /// <summary>
    /// Provides extension methods for <see cref="Result{T}"/>
    /// </summary>
    public static class IResultExtensions
    {
        /// <summary>
        /// Deconstructs the specified <paramref name="result"/> into contained data and exception.
        /// </summary>
        public static void Deconstruct<TData>(this IResult<TData> result, out TData? data, out Exception? exception) where TData : class
        {
            if (result is null) throw new ArgumentNullException(nameof(result));

            data = result.Data;
            exception = result.Exception;
        }

        /// <summary>
        /// Tests the specified <paramref name="result"/> for success.
        /// If the result is successful, yields the contained <paramref name="data"/> (otherwise null).
        /// Else, yields the contained <paramref name="exception"/> (otherwise null).
        /// </summary>
        public static bool Try<TData>(this IResult<TData> result, out TData? data, out Exception? exception) where TData : class
        {
            if (result is null) throw new ArgumentNullException(nameof(result));

            data = result.Data;
            exception = result.Exception;
            return result.Success;
        }

        /// <summary>
        /// Tests the specified <paramref name="result"/> for success.
        /// If the result is successful, yields the contained <paramref name="data"/> (otherwise null).
        /// </summary>
        public static bool Try<TData>(this IResult<TData> result, out TData? data) where TData : class
        {
            if (result is null) throw new ArgumentNullException(nameof(result));

            return result.Try(out data, out _);
        }

        /// <summary>
        /// Tests the specified <paramref name="result"/> for success.
        /// If the result is not successful, yields the contained <see cref="Exception"/> (otherwise null).
        /// </summary>
        public static bool Try<TData>(this IResult<TData> result, out Exception? ex) where TData : class
        {
            if (result is null) throw new ArgumentNullException(nameof(result));

            return result.Try(out _, out ex);
        }

        /// <summary>
        /// Tests the specified <paramref name="result"/> for success.
        /// If successful, passes the contained data.
        /// Else, throws an <see cref="ResultException"/> containing the underlying <see cref="Exception"/>
        /// </summary>
        public static TData Validate<TData>(this IResult<TData> result) where TData : class
        {
            if (result is null) throw new ArgumentNullException(nameof(result));

            if (result.Try(out var data, out var ex))
                return data!;
            else
                throw new ResultException(ex!);
        }

        /// <summary>
        /// Executes the specified <paramref name="resultTask"/> asynchronously,
        /// and tests its result for success.
        /// If successful, passes the contained data;
        /// Else, throws an <see cref="ResultException"/> containing the underlying <see cref="Exception"/>
        /// </summary>
        public static async Task<TData> ValidateAsync<TData>(this Task<IResult<TData>> resultTask) where TData : class
        {
            if (resultTask is null) throw new ArgumentNullException(nameof(resultTask));

            return Validate(await resultTask.ConfigureAwait(false));
        }

        /// <summary>
        /// Executes the specified <paramref name="resultTask"/> synchronously,
        /// and tests its result for success.
        /// If successful, passes the contained data;
        /// Else, throws an <see cref="ResultException"/> containing the underlying <see cref="Exception"/>
        /// </summary>
        public static TData Validate<TData>(this Task<IResult<TData>> resultTask) where TData : class
        {
            if (resultTask is null) throw new ArgumentNullException(nameof(resultTask));

            return Validate(resultTask.Result);
        }

        /// <summary>
        /// Tests the specified <paramref name="result"/> for success.
        /// If successfull, invokes the specified <paramref name="dataHandler"/>
        /// with the contained data.
        /// Else, invokes the specified <paramref name="errorHandler"/> 
        /// with the contained error.
        /// </summary>
        public static void Handle<TData>(
            this IResult<TData> result,
            Action<TData> dataHandler,
            Action<Exception> errorHandler
        ) where TData : class
        {
            if (result is null) throw new ArgumentNullException(nameof(result));
            if (dataHandler is null) throw new ArgumentNullException(nameof(dataHandler));
            if (errorHandler is null) throw new ArgumentNullException(nameof(errorHandler));

            if (result.Try(out var data, out var ex))
                dataHandler(data!);
            else
                errorHandler(ex!);
        }

        /// <summary>
        /// Executes the specified <paramref name="resultTask"/> asynchronously,
        /// and tests its result for success.
        /// If successfull, invokes the specified <paramref name="dataHandler"/>
        /// with the contained data.
        /// Else, invokes the specified <paramref name="errorHandler"/> 
        /// with the contained error.
        /// </summary>
        public static async Task HandleAsync<TData>(
            this Task<IResult<TData>> resultTask,
            Action<TData> dataHandler,
            Action<Exception> errorHandler
        ) where TData : class
        {
            if (resultTask is null) throw new ArgumentNullException(nameof(resultTask));

            Handle(await resultTask.ConfigureAwait(false), dataHandler, errorHandler);
        }

        /// <summary>
        /// Executes the specified <paramref name="resultTask"/> asynchronously,
        /// and tests its result for success.
        /// If successfull, invokes the specified <paramref name="dataHandler"/>
        /// with the contained data.
        /// Else, invokes the specified <paramref name="errorHandler"/> 
        /// with the contained error.
        /// </summary>
        public static async Task HandleAsync<TData>(
            this Task<IResult<TData>> resultTask,
            Action<Task<TData>> dataHandler,
            Action<Task<Exception>> errorHandler
        ) where TData : class
        {
            if (resultTask is null) throw new ArgumentNullException(nameof(resultTask));

            await HandleAsync(resultTask, dataHandler, errorHandler).ConfigureAwait(false) ;
        }

        /// <summary>
        /// Executes the specified <paramref name="resultTask"/> synchronously,
        /// and tests its result for success.
        /// If successfull, invokes the specified <paramref name="dataHandler"/>
        /// with the contained data.
        /// Else, invokes the specified <paramref name="errorHandler"/> 
        /// with the contained error.
        /// </summary>
        public static void Handle<TData>(
           this Task<IResult<TData>> resultTask,
           Action<TData> dataHandler,
           Action<Exception> errorHandler
        ) where TData : class
        {
            if (resultTask is null) throw new ArgumentNullException(nameof(resultTask));

            Handle(resultTask.Result, dataHandler, errorHandler);
        }

        /// <summary>
        /// Tests the specified <paramref name="result"/> for success.
        /// If successfull, invokes the specified <paramref name="dataMapper"/>
        /// with the contained data, and returns its result.
        /// Else, invokes the specified <paramref name="errorMapper"/> 
        /// with the contained error, and returns its result.
        /// </summary>
        public static TResult Map<TData, TResult>(
            this IResult<TData> result,
            Func<TData, TResult> dataMapper,
            Func<Exception, TResult> errorMapper
        ) where TData : class
        {
            if (result is null) throw new ArgumentNullException(nameof(result));
            if (dataMapper is null) throw new ArgumentNullException(nameof(dataMapper));
            if (errorMapper is null) throw new ArgumentNullException(nameof(errorMapper));

            if (result.Try(out var data, out var ex))
                return dataMapper(data!);
            else
                return errorMapper(ex!);
        }

        /// <summary>
        /// Executes the specified <paramref name="resultTask"/> asynchronously,
        /// and tests its result for success.
        /// If successfull, invokes the specified <paramref name="dataMapper"/>
        /// with the contained data, and returns its result.
        /// Else, invokes the specified <paramref name="errorMapper"/> 
        /// with the contained error, and returns its result.
        /// </summary>
        public static async Task<TResult> MapAsync<TData, TResult>(
            this Task<IResult<TData>> resultTask,
            Func<TData, TResult> dataMapper,
            Func<Exception, TResult> errorMapper
        ) where TData : class
        {
            if (resultTask is null) throw new ArgumentNullException(nameof(resultTask));

            return Map(await resultTask.ConfigureAwait(false), dataMapper, errorMapper);
        }

        /// <summary>
        /// Executes the specified <paramref name="resultTask"/> asynchronously,
        /// and tests its result for success.
        /// If successfull, invokes the specified <paramref name="dataMapper"/>
        /// with the contained data, and returns its result.
        /// Else, invokes the specified <paramref name="errorMapper"/> 
        /// with the contained error, and returns its result.
        /// </summary>
        public static async Task<TResult> MapAsync<TData, TResult>(
            this Task<IResult<TData>> resultTask,
            Func<TData, Task<TResult>> dataMapper,
            Func<Exception, Task<TResult>> errorMapper
        ) where TData : class
        {
            if (resultTask is null) throw new ArgumentNullException(nameof(resultTask));

            return await MapAsync(resultTask, dataMapper, errorMapper).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the specified <paramref name="resultTask"/> synchronously,
        /// and tests its result for success.
        /// If successfull, invokes the specified <paramref name="dataMapper"/>
        /// with the contained data, and returns its result.
        /// Else, invokes the specified <paramref name="errorMapper"/> 
        /// with the contained error, and returns its result.
        /// </summary>
        public static TResult Map<TData, TResult>(
            this Task<IResult<TData>> resultTask,
            Func<TData, TResult> dataMapper,
            Func<Exception, TResult> errorMapper
        ) where TData : class
        {
            if (resultTask is null) throw new ArgumentNullException(nameof(resultTask));

            return Map(resultTask.Result, dataMapper, errorMapper);
        }
    }
}


