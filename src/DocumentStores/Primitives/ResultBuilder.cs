using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable enable

namespace DocumentStores.Primitives
{
    
    internal static class ResultBuilder
    {
        public static async Task<Result> BuildResultAsync(
            Func<Task> @function,
            Func<Exception, bool> exceptionFilter,
            IEnumerable<TimeSpan> retrySpansProvider)
        {
            async Task<Result> GetResultAsync()
            {
                try
                {
                    await @function.Invoke();
                    return Result.Ok();
                }
                catch (Exception _) when (exceptionFilter(_))
                {
                    return Result.Error(_);
                }
            }

            var result = await GetResultAsync();
            foreach (var retrySpan in retrySpansProvider)
            {
                if (result.Try()) return result;
                await Task.Delay(retrySpan);
                result = await GetResultAsync();
            }
            return result;
        }

        public static async Task<Result<TData>> BuildResultAsync<TData>(
            Func<Task<TData>> @function,
            Func<Exception, bool> exceptionFilter,
            IEnumerable<TimeSpan> retrySpansProvider) where TData : class
        {
            async Task<Result<TData>> GetResultAsync()
            {
                try
                {
                    var data = await @function.Invoke();
                    return data;
                }
                catch (Exception _) when (exceptionFilter(_))
                {
                    return _;
                }
            }

            var result = await GetResultAsync();
            foreach (var retrySpan in retrySpansProvider)
            {
                if (result.Try()) return result;
                await Task.Delay(retrySpan);
                result = await GetResultAsync();
            }
            return result;
        }

        internal static IEnumerable<TimeSpan> GetIncrementalTimeSpans(TimeSpan seed, uint count) =>
            Enumerable
                .Range(0, (int)count)
                .Select(i => TimeSpan.FromMilliseconds(seed.TotalMilliseconds * Math.Pow(2, i)));

        internal static IEnumerable<TimeSpan> GetConstantTimeSpans(TimeSpan seed, uint count) =>
            Enumerable
                .Range(0, (int)count)
                .Select(_ => seed);

    }

}