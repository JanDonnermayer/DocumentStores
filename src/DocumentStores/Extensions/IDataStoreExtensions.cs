﻿using System.Security.Cryptography;
using System.Threading.Tasks;
using DocumentStores.Internal;

namespace DocumentStores
{
    internal static class IDataStoreExtensions
    {
        public static IDataChannel ToChannel(this IDataStore store, DocumentAddress address) =>
            new DataChannel(store, address);

        public static async Task CopyAsync(this IDataStore store, DocumentAddress sourceAddress, DocumentAddress targetAddress)
        {
            var source = store.ToChannel(sourceAddress);
            var target = store.ToChannel(targetAddress);

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