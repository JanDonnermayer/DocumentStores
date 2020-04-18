using Microsoft.Extensions.DependencyInjection;

namespace DocumentStores
{
    /// <summary>
    /// An instance of <see cref="IServiceCollection"/>
    /// containing a service descriptor for <typeparam name="TService"/>
    /// </summary>
    public interface IGenericServiceCollection<out TService> : IServiceCollection { }
}