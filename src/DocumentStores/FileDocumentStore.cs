using System;
using System.Threading.Tasks;
using DocumentStores.Primitives;
using DocumentStores.Internal;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using Newtonsoft.Json;

namespace DocumentStores
{
    /// <summary/> 
    public class FileDocumentStore<TFileHandling> : IDocumentStore
        where TFileHandling : IFileHandling, new()

    {
        private readonly IDocumentStoreInternal store;

        /// <summary/> 
        public FileDocumentStore(string directory)
        {
            var fileHandling = new TFileHandling();
            this.store = new FileDocumentStoreInternal(directory, fileHandling);
        }

        #region Private members

        private Func<Func<Task<T>>, Func<Task<Result<T>>>> Catch<T>() where T : class =>
            producer =>
                producer.Catch(
                    exceptionFilter: ex =>
                        ex is DocumentException
                        || ex is IOException);

        private Func<Func<Task<Result<T>>>, Func<Task<Result<T>>>> Retry<T>() where T : class =>
            producer =>
                producer.RetryIncrementally(
                    frequencySeed: TimeSpan.FromMilliseconds(50),
                    count: 5,
                    exceptionFilter: ex => ex is IOException);


        #endregion


        #region Implementation of IDocumentStore

        /// <summary/> 
        public Task<IEnumerable<string>> GetKeysAsync<TData>(CancellationToken ct = default) where TData : class =>
            store.GetKeysAsync<TData>(ct);

        /// <summary/> 
        public async Task<Result<T>> AddOrUpdateDocumentAsync<T>(string key,
            Func<string, Task<T>> addDataAsync, Func<string, T, Task<T>> updateDataAsync) where T : class =>
                 await Function.ApplyArgs(store.AddOrUpdateDocumentAsync, key, addDataAsync, updateDataAsync)
                        .Init(Catch<T>())
                        .Pipe(Retry<T>())
                        .Invoke()
                        .ConfigureAwait(false);
                        
        /// <summary/> 
        public async Task<Result<T>> GetOrAddDocumentAsync<T>(string key,
            Func<string, Task<T>> addDataAsync) where T : class =>
                await Function.ApplyArgs(store.GetOrAddDocumentAsync, key, addDataAsync)
                        .Init(Catch<T>())
                        .Pipe(Retry<T>())
                        .Invoke()
                        .ConfigureAwait(false);

        /// <summary/> 
        public async Task<Result<T>> GetDocumentAsync<T>(string key) where T : class =>
            await Function.ApplyArgs(store.GetDocumentAsync<T>, key)
                    .Init(Catch<T>())
                    .Pipe(Retry<T>())
                    .Invoke()
                    .ConfigureAwait(false);

        /// <summary/> 
        public async Task<Result<Unit>> DeleteDocumentAsync<T>(string key) where T : class =>
            await Function.ApplyArgs(store.DeleteDocumentAsync<T>, key)
                    .Init(Catch<Unit>())
                    .Pipe(Retry<Unit>())
                    .Invoke()
                    .ConfigureAwait(false);

        /// <summary/> 
        public async Task<Result<Unit>> PutDocumentAsync<T>(string key, T data) where T : class =>
            await Function.ApplyArgs<string, T, Task<Unit>>(store.PutDocumentAsync, key, data)
                    .Init(Catch<Unit>())
                    .Pipe(Retry<Unit>())
                    .Invoke()
                    .ConfigureAwait(false);

        #endregion
    }

}
