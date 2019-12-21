using System.Security.Cryptography;
using System.Threading.Tasks;
using DocumentStores.Internal;
using DocumentStores.Primitives;

namespace DocumentStores
{
    internal static class IDataStoreExtensions
    {
        public static IDataProxy CreateProxy(this IDataStore store, DocumentAddress address) =>
            new DataProxy(store, address);

        public static async Task CopyAsync(this IDataStore store, DocumentAddress sourceAddress, DocumentAddress targetAddress)
        {
            var source = store.CreateProxy(sourceAddress);
            var target = store.CreateProxy(targetAddress);

            if (target.Exists()) target.Delete();

            await source
                .GetReadStream()
                .CopyToAsync(target.GetWriteStream())
                .ConfigureAwait(false);
        }

        public static async Task MoveAsync(this IDataStore store, DocumentAddress sourceAddress, DocumentAddress targetAddress)
        {
            await store.CopyAsync(sourceAddress, targetAddress).ConfigureAwait(false);
            store.Delete(sourceAddress);
        }
    }


}