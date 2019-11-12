using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentStores.Primitives;

namespace DocumentStores.Abstractions
{
    public static class IDocumentStoreExtensions
    {

        public static IDocumentStore<TData> AsTypedDocumentStore<TData>(this IDocumentStore source) where TData : class =>
            new DocumentStoreTypedWrapper<TData>(source);

        internal class DocumentStoreTypedWrapper<TData> : IDocumentStore<TData> where TData : class
        {
            private readonly IDocumentStore source;

            public DocumentStoreTypedWrapper(IDocumentStore source) =>
                this.source = source ?? throw new ArgumentNullException(nameof(source));

            Task<Result<TData>> IDocumentStore<TData>.AddOrUpdateDocumentAsync(
                string key,
                Func<string, Task<TData>> addDataAsync,
                Func<string, TData, Task<TData>> updateDataAsync) =>
                source.AddOrUpdateDocumentAsync(key, addDataAsync, updateDataAsync);

            Task<Result<TData>> IDocumentStore<TData>.GetOrAddDocumentAsync(
                string key,
                Func<string, Task<TData>> addDataAsync) =>
                source.GetOrAddDocumentAsync(key, addDataAsync);

            Task<Result> IDocumentStore<TData>.DeleteDocumentAsync(string key) => 
                source.DeleteDocumentAsync<TData>(key);

            Task<Result<TData>> IDocumentStore<TData>.GetDocumentAsync(string key) =>
                source.GetDocumentAsync<TData>(key);

            Task<IEnumerable<string>> IDocumentStore<TData>.GetKeysAsync() =>
                source.GetKeysAsync<TData>();

            Task<Result> IDocumentStore<TData>.PutDocumentAsync(string key, TData data) =>
                source.PutDocumentAsync(key, data);
        }

    }
}