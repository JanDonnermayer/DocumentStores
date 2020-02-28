using System.Threading.Tasks;
using DocumentStores;

namespace System
{
    public static class ResultExtensions
    {
        public static void Match<T>(
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

        public static V Match<T, V>(
            this Result<T> result,
            Func<T, V> dataHandler,
            Func<Exception, V> errorHandler
        ) where T : class
        {
            if (result is null) throw new ArgumentNullException(nameof(result));
            if (dataHandler is null) throw new ArgumentNullException(nameof(dataHandler));
            if (errorHandler is null) throw new ArgumentNullException(nameof(errorHandler));

            if (result.Try(out var data, out var ex))
                return dataHandler(data!);
            else
                return errorHandler(ex!);
        }

        public static async Task MatchAsync<T>(
            this Task<Result<T>> resultTask,
            Action<T> dataHandler,
            Action<Exception> errorHandler
        ) where T : class
        {
            if (resultTask is null) throw new ArgumentNullException(nameof(resultTask));
            if (dataHandler is null) throw new ArgumentNullException(nameof(dataHandler));
            if (errorHandler is null) throw new ArgumentNullException(nameof(errorHandler));

            Match(await resultTask.ConfigureAwait(false), dataHandler, errorHandler);
        }

        public static async Task<V> MatchAsync<T, V>(
            this Task<Result<T>> resultTask,
            Func<T, V> dataHandler,
            Func<Exception, V> errorHandler
        ) where T : class
        {
            if (resultTask is null) throw new ArgumentNullException(nameof(resultTask));
            if (dataHandler is null) throw new ArgumentNullException(nameof(dataHandler));
            if (errorHandler is null) throw new ArgumentNullException(nameof(errorHandler));

            return Match(await resultTask.ConfigureAwait(false), dataHandler, errorHandler);
        }
    }
}


