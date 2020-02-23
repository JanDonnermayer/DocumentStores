using System;
using System.Threading.Tasks;
using DocumentStores.Internal;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using Newtonsoft.Json;
using System.ComponentModel;

namespace DocumentStores.Internal
{
    internal class DocumentStore : IDocumentStore
    {
        private readonly IDocumentStoreInternal store;

        public DocumentStore(IDocumentSerializer serializer, IDataStore store)
        {
            this.store = new DocumentStoreInternal(serializer, store);
        }

        #region Private members

        private Func<Func<Task<T>>, Func<Task<Result<T>>>> Catch<T>() where T : class =>
            producer =>
                producer.Catch(
                    exceptionFilter: ex =>
                        ex is IOException 
                        || ex is UnauthorizedAccessException
                        || ex is DocumentException);

        private Func<Func<Task<Result<T>>>, Func<Task<Result<T>>>> Retry<T>() where T : class =>
            producer =>
                producer.RetryIncrementally(
                    frequencySeed: TimeSpan.FromMilliseconds(50),
                    count: 5,
                    exceptionFilter: ex => ex is IOException);

        #endregion

        #region Implementation of IDocumentStore

        Task<IEnumerable<DocumentAddress>> IDocumentStore.GetAddressesAsync<TData>(
            DocumentRoute topicName,
            DocumentSearchOption options,
            CancellationToken ct) where TData : class =>
                store.GetAddressesAsync<TData>(topicName, options, ct);

        async Task<Result<T>> IDocumentStore.AddOrUpdateAsync<T>(DocumentAddress address,
            Func<DocumentAddress, Task<T>> addDataAsync, Func<DocumentAddress, T, Task<T>> updateDataAsync) where T : class =>
                 await Function.ApplyArgs(store.AddOrUpdateAsync, address, addDataAsync, updateDataAsync)
                        .Init(Catch<T>())
                        .Pipe(Retry<T>())
                        .Invoke()
                        .ConfigureAwait(false);

        async Task<Result<T>> IDocumentStore.GetOrAddAsync<T>(DocumentAddress address,
            Func<DocumentAddress, Task<T>> addDataAsync) where T : class =>
                await Function.ApplyArgs(store.GetOrAddAsync, address, addDataAsync)
                        .Init(Catch<T>())
                        .Pipe(Retry<T>())
                        .Invoke()
                        .ConfigureAwait(false);

        async Task<Result<T>> IDocumentStore.GetAsync<T>(DocumentAddress address) where T : class =>
            await Function.ApplyArgs(store.GetAsync<T>, address)
                    .Init(Catch<T>())
                    .Pipe(Retry<T>())
                    .Invoke()
                    .ConfigureAwait(false);

        async Task<Result<Unit>> IDocumentStore.DeleteAsync<T>(DocumentAddress address) where T : class =>
            await Function.ApplyArgs(store.DeleteAsync<T>, address)
                    .Init(Catch<Unit>())
                    .Pipe(Retry<Unit>())
                    .Invoke()
                    .ConfigureAwait(false);

        async Task<Result<Unit>> IDocumentStore.PutAsync<T>(DocumentAddress address, T data) where T : class =>
            await Function.ApplyArgs(store.PutAsync, address, data)
                    .Init(Catch<Unit>())
                    .Pipe(Retry<Unit>())
                    .Invoke()
                    .ConfigureAwait(false);

        #endregion
    }

}
