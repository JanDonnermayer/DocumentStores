using System;
using System.Threading.Tasks;
using DocumentStores.Primitives;
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
        private readonly IDocumentStoreAdapter store;

        public DocumentStore(IDocumentSerializer serializer, IDocumentStoreInternal router)
        {
            this.store = new DocumentStoreAdapter(serializer, router);
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
            DocumentSearchOptions options,
            CancellationToken ct) where TData : class =>
                store.GetAddressesAsync<TData>(topicName, options, ct);

        async Task<Result<T>> IDocumentStore.AddOrUpdateDocumentAsync<T>(DocumentAddress address,
            Func<DocumentAddress, Task<T>> addDataAsync, Func<DocumentAddress, T, Task<T>> updateDataAsync) where T : class =>
                 await Function.ApplyArgs(store.AddOrUpdateDocumentAsync, address, addDataAsync, updateDataAsync)
                        .Init(Catch<T>())
                        .Pipe(Retry<T>())
                        .Invoke()
                        .ConfigureAwait(false);

        async Task<Result<T>> IDocumentStore.GetOrAddDocumentAsync<T>(DocumentAddress address,
            Func<DocumentAddress, Task<T>> addDataAsync) where T : class =>
                await Function.ApplyArgs(store.GetOrAddDocumentAsync, address, addDataAsync)
                        .Init(Catch<T>())
                        .Pipe(Retry<T>())
                        .Invoke()
                        .ConfigureAwait(false);

        async Task<Result<T>> IDocumentStore.GetDocumentAsync<T>(DocumentAddress address) where T : class =>
            await Function.ApplyArgs(store.GetDocumentAsync<T>, address)
                    .Init(Catch<T>())
                    .Pipe(Retry<T>())
                    .Invoke()
                    .ConfigureAwait(false);

        async Task<Result<Unit>> IDocumentStore.DeleteDocumentAsync<T>(DocumentAddress address) where T : class =>
            await Function.ApplyArgs(store.DeleteDocumentAsync<T>, address)
                    .Init(Catch<Unit>())
                    .Pipe(Retry<Unit>())
                    .Invoke()
                    .ConfigureAwait(false);

        async Task<Result<Unit>> IDocumentStore.PutDocumentAsync<T>(DocumentAddress address, T data) where T : class =>
            await Function.ApplyArgs(store.PutDocumentAsync, address, data)
                    .Init(Catch<Unit>())
                    .Pipe(Retry<Unit>())
                    .Invoke()
                    .ConfigureAwait(false);

        #endregion
    }

}
