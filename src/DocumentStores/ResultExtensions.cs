using DocumentStores.Primitives;
using System;
using System.Threading.Tasks;

#nullable enable

namespace DocumentStores
{
    internal static class ResultExtensions
    {


        internal static async Task Test()
        {
            Task<Result<string>> f1() => ResultBuilder.Catch(() => Task.FromResult("lol"));
            Task<Result<string>> f2(string arg) => ResultBuilder.Catch(() => Task.FromResult(arg + "lol"));
            Task<Result> f3(string arg) => ResultBuilder.Catch(() => Task.CompletedTask);

            //var r2 = ResultBuilder.BuildResultAsync(f2);
            await f1()
                     .Then(f2)
                     .Do(f3)
                     .Then(f2)
                     .Do(f3)
                     .Then(f2);


        }

    }

}