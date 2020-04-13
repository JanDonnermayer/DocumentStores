using DocumentStores.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DocumentStores
{
    /// <inheritdoc/>
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Adds an <see cref="JsonFileDocumentStore"/> as <see cref="IDocumentStore"/>
        /// to the <see cref="IServiceCollection"/>
        /// </summary>
        /// <param name="services">The services to which the <see cref="JsonFileDocumentStore"/> is added.</param>
        /// <param name="directory">The directory that is used to store documents</param>
        /// <returns></returns>
        public static IDocumentTopicBuilderServiceCollection AddJsonFileDocumentStore(this IServiceCollection services, string directory)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));
            if (string.IsNullOrEmpty(directory)) throw new ArgumentException("Value cannot be null or empty.", nameof(directory));

            services.AddSingleton<IDocumentStore>(_ => new JsonFileDocumentStore(directory));
            return new DocumentTopicBuilder(services);
        }

        #region Private Types

        private class DocumentTopicBuilder : IDocumentTopicBuilderServiceCollection
        {
            private readonly IServiceCollection services;

            public DocumentTopicBuilder(IServiceCollection services) =>
                this.services = services ?? throw new ArgumentNullException(nameof(services));

            public IDocumentTopicBuilderServiceCollection AddTopic<TData>() where TData : class
            {
                this.AddSingleton<IDocumentTopic<TData>, DocumentTopic<TData>>();
                return this;
            }

            #region  IServiceCollection

            public ServiceDescriptor this[int index] { get => services[index]; set => services[index] = value; }

            public int Count => services.Count;

            public bool IsReadOnly => services.IsReadOnly;

            public void Add(ServiceDescriptor item) => services.Add(item);

            public void Clear() => services.Clear();

            public bool Contains(ServiceDescriptor item) => services.Contains(item);

            public void CopyTo(ServiceDescriptor[] array, int arrayIndex) => services.CopyTo(array, arrayIndex);

            public IEnumerator<ServiceDescriptor> GetEnumerator() => services.GetEnumerator();

            public int IndexOf(ServiceDescriptor item) => services.IndexOf(item);

            public void Insert(int index, ServiceDescriptor item) => services.Insert(index, item);

            public bool Remove(ServiceDescriptor item) => services.Remove(item);

            public void RemoveAt(int index) => services.RemoveAt(index);

            IEnumerator IEnumerable.GetEnumerator() => services.GetEnumerator();

            #endregion
        }

        #endregion

    }
}