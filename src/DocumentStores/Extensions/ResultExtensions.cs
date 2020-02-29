using System.Threading.Tasks;
using DocumentStores;

namespace System
{
    /// <summary>
    /// Provides extension methods for <see cref="Result{T}"/>
    /// </summary>
    public static class ResultExtensions
    {
        /// <summary>
        /// Tests the specified <paramref name="result"/> for success.
        /// If successfull, invokes the specified <paramref name="dataHandler"/>
        /// with the contained data.
        /// Else: Invokes the specified <paramref name="errorHandler"/> 
        /// with the contained error.
        /// </summary>
        public static void Handle<TData>(
            this Result<TData> result,
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
        /// Tests the specified <paramref name="result"/> for success.
        /// If successfull, invokes the specified <paramref name="dataMapper"/>
        /// with the contained data, and returns its result.
        /// Else: Invokes the specified <paramref name="errorMapper"/> 
        /// with the contained error, and returns its result.
        /// </summary>
        public static TResult Map<TData, TResult>(
            this Result<TData> result,
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
        /// and test its result for success.
        /// If successfull, invokes the specified <paramref name="dataHandler"/>
        /// with the contained data.
        /// Else: Invokes the specified <paramref name="errorHandler"/> 
        /// with the contained error.
        /// </summary>
        public static async Task HandleAsync<TData>(
            this Task<Result<TData>> resultTask,
            Action<TData> dataHandler,
            Action<Exception> errorHandler
        ) where TData : class
        {
            if (resultTask is null) throw new ArgumentNullException(nameof(resultTask));

            Handle(await resultTask.ConfigureAwait(false), dataHandler, errorHandler);
        }

        /// <summary>
        /// Executes the specified <paramref name="resultTask"/> asynchronously,
        /// and test its result for success.
        /// If successfull, invokes the specified <paramref name="dataMapper"/>
        /// with the contained data, and returns its result.
        /// Else: Invokes the specified <paramref name="errorMapper"/> 
        /// with the contained error, and returns its result.
        /// </summary>
        public static async Task<TResult> MapAsync<TData, TResult>(
            this Task<Result<TData>> resultTask,
            Func<TData, TResult> dataMapper,
            Func<Exception, TResult> errorMapper
        ) where TData : class
        {
            if (resultTask is null) throw new ArgumentNullException(nameof(resultTask));

            return Map(await resultTask.ConfigureAwait(false), dataMapper, errorMapper);
        }

        /// <summary>
        /// Executes the specified <paramref name="resultTask"/> synchronously,
        /// and test its result for success.
        /// If successfull, invokes the specified <paramref name="dataHandler"/>
        /// with the contained data.
        /// Else: Invokes the specified <paramref name="errorHandler"/> 
        /// with the contained error.
        /// </summary>
         public static void HandleResult<TData>(
            this Task<Result<TData>> resultTask,
            Action<TData> dataHandler,
            Action<Exception> errorHandler
        ) where TData : class
        {
            if (resultTask is null) throw new ArgumentNullException(nameof(resultTask));

            Handle(resultTask.Result, dataHandler, errorHandler);
        }

        /// <summary>
        /// Executes the specified <paramref name="resultTask"/> synchronously,
        /// and test its result for success.
        /// If successfull, invokes the specified <paramref name="dataMapper"/>
        /// with the contained data, and returns its result.
        /// Else: Invokes the specified <paramref name="errorMapper"/> 
        /// with the contained error, and returns its result.
        /// </summary>
        public static TResult MapResult<TData, TResult>(
            this Task<Result<TData>> resultTask,
            Func<TData, TResult> dataMapper,
            Func<Exception, TResult> errorMapper
        ) where TData : class
        {
            if (resultTask is null) throw new ArgumentNullException(nameof(resultTask));

            return Map(resultTask.Result, dataMapper, errorMapper);
        }
    }
}


