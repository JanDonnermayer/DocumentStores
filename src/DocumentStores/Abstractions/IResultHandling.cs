using System;
using System.ComponentModel;
using System.Threading.Tasks;
using DocumentStores.Primitives;

namespace DocumentStores
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IResultHandling
    {        
        Func<Func<Task<T>>, Func<Task<Result<T>>>> Catch<T>() where T : class;

        Func<Func<Task<Result<T>>>, Func<Task<Result<T>>>> Retry<T>() where T : class;
    }
}