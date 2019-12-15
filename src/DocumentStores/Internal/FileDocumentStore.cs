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

    internal class FileDocumentStore : IDocumentStore       
    {
        private readonly IDocumentStoreInternal store;

        public FileDocumentStore(string directory, IFileHandling handling)
        {
            this.store = new FileDocumentStoreInternal(directory, handling);
        }

        #region Private members

        private Func<Func<Task<T>>, Func<Task<Result<T>>>> Catch<T>() where T : class =>
            producer =>
                producer.Catch(
                    exceptionFilter: ex =>
                        ex is DocumentException
                        || ex is IOException
                        || ex is UnauthorizedAccessException);

        private Func<Func<Task<Result<T>>>, Func<Task<Result<T>>>> Retry<T>() where T : class =>
            producer =>
                producer.RetryIncrementally(
                    frequencySeed: TimeSpan.FromMilliseconds(50),
                    count: 5,
                    exceptionFilter: ex => ex is IOException);


        #endregion


        #region Implementation of IDocumentStore

        public Task<IEnumerable<DocumentAddress>> GetAddressesAsync<TData>(
            DocumentRoute topicName, 
            DocumentSearchOptions options = DocumentSearchOptions.TopLevelOnly,
            CancellationToken ct = default) where TData : class =>
                store.GetAddressesAsync<TData>(topicName, options, ct);

        public async Task<Result<T>> AddOrUpdateDocumentAsync<T>(DocumentAddress address,
            Func<string, Task<T>> addDataAsync, Func<string, T, Task<T>> updateDataAsync) where T : class =>
                 await Function.ApplyArgs(store.AddOrUpdateDocumentAsync, address, addDataAsync, updateDataAsync)
                        .Init(Catch<T>())
                        .Pipe(Retry<T>())
                        .Invoke()
                        .ConfigureAwait(false);

        public async Task<Result<T>> GetOrAddDocumentAsync<T>(DocumentAddress address,
            Func<string, Task<T>> addDataAsync) where T : class =>
                await Function.ApplyArgs(store.GetOrAddDocumentAsync, address, addDataAsync)
                        .Init(Catch<T>())
                        .Pipe(Retry<T>())
                        .Invoke()
                        .ConfigureAwait(false);

        public async Task<Result<T>> GetDocumentAsync<T>(DocumentAddress address) where T : class =>
            await Function.ApplyArgs(store.GetDocumentAsync<T>, address)
                    .Init(Catch<T>())
                    .Pipe(Retry<T>())
                    .Invoke()
                    .ConfigureAwait(false);

        public async Task<Result<Unit>> DeleteDocumentAsync<T>(DocumentAddress address) where T : class =>
            await Function.ApplyArgs(store.DeleteDocumentAsync<T>, address)
                    .Init(Catch<Unit>())
                    .Pipe(Retry<Unit>())
                    .Invoke()
                    .ConfigureAwait(false);

        public async Task<Result<Unit>> PutDocumentAsync<T>(DocumentAddress address, T data) where T : class =>
            await Function.ApplyArgs(store.PutDocumentAsync, address, data)
                    .Init(Catch<Unit>())
                    .Pipe(Retry<Unit>())
                    .Invoke()
                    .ConfigureAwait(false);

        #endregion
    }

}
