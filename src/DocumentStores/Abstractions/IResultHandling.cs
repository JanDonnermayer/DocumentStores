using System;
using System.Threading.Tasks;
using DocumentStores.Primitives;

namespace DocumentStores
{
    interface IResultHandling
    {        
        Func<Func<Task<T>>, Func<Task<Result<T>>>> Catch<T>() where T : class;

        Func<Func<Task<Result<T>>>, Func<Task<Result<T>>>> Retry<T>() where T : class;
    }
}