using System.Threading.Tasks;
using DocumentStores;

namespace System
{
    public static class ResultExtensions
    {
        public static void Handle<T>(
            this Result<T> result,
            Action<T> dataHandler,
            Action<Exception> errorHandler
        ) where T : class
        {
            if (result is null) throw new ArgumentNullException(nameof(result));
            if (dataHandler is null) throw new ArgumentNullException(nameof(dataHandler));
            if (errorHandler is null) throw new ArgumentNullException(nameof(errorHandler));

            if (result.Try(out var data, out var ex))
                dataHandler(data!);
            else
                errorHandler(ex!);
        }

        public static V Map<T, V>(
            this Result<T> result,
            Func<T, V> dataMapper,
            Func<Exception, V> errorMapper
        ) where T : class
        {
            if (result is null) throw new ArgumentNullException(nameof(result));
            if (dataMapper is null) throw new ArgumentNullException(nameof(dataMapper));
            if (errorMapper is null) throw new ArgumentNullException(nameof(errorMapper));

            if (result.Try(out var data, out var ex))
                return dataMapper(data!);
            else
                return errorMapper(ex!);
        }

        public static async Task HandleAsync<T>(
            this Task<Result<T>> resultTask,
            Action<T> dataHandler,
            Action<Exception> errorHandler
        ) where T : class
        {
            if (resultTask is null) throw new ArgumentNullException(nameof(resultTask));

            Handle(await resultTask.ConfigureAwait(false), dataHandler, errorHandler);
        }

        public static async Task<V> MapAsync<T, V>(
            this Task<Result<T>> resultTask,
            Func<T, V> dataMapper,
            Func<Exception, V> errorMapper
        ) where T : class
        {
            if (resultTask is null) throw new ArgumentNullException(nameof(resultTask));

            return Map(await resultTask.ConfigureAwait(false), dataMapper, errorMapper);
        }

         public static void HandleResult<T>(
            this Task<Result<T>> resultTask,
            Action<T> dataHandler,
            Action<Exception> errorHandler
        ) where T : class
        {
            if (resultTask is null) throw new ArgumentNullException(nameof(resultTask));

            Handle(resultTask.Result, dataHandler, errorHandler);
        }

        public static V MapResult<T, V>(
            this Task<Result<T>> resultTask,
            Func<T, V> dataMapper,
            Func<Exception, V> errorMapper
        ) where T : class
        {
            if (resultTask is null) throw new ArgumentNullException(nameof(resultTask));

            return Map(resultTask.Result, dataMapper, errorMapper);
        }
    }
}


