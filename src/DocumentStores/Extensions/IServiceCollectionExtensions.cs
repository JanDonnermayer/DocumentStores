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
        public static IDocumentTopicBuilder AddJsonFileDocumentStore(this IServiceCollection services, string directory)
        {
            if (services is null) throw new System.ArgumentNullException(nameof(services));
            if (string.IsNullOrEmpty(directory)) throw new System.ArgumentException("message", nameof(directory));

            services.AddSingleton<IDocumentStore>(_ => new JsonFileDocumentStore(directory));
            return new DocumentTopicBuilder(services);
        }

        #region Private Types

        private class DocumentTopicBuilder : IServiceCollection, IDocumentTopicBuilder
        {
            private readonly IServiceCollection services;

            public DocumentTopicBuilder(IServiceCollection services) =>
                this.services = services ?? throw new ArgumentNullException(nameof(services));


            public IDocumentTopicBuilder WithObservableOn<TData>() where TData : class
            {
                this.AddSingleton<IDocumentTopic<TData>, DocumentTopic<TData>>();
                return this;
            }

            #region  IServiceCollection

            public ServiceDescriptor this[int index] { get => services[index]; set => services[index] = value; }

            public int Count => services.Count;

            public bool IsReadOnly => services.IsReadOnly;

            public void Add(ServiceDescriptor item)
            {
                services.Add(item);
            }

            public void Clear()
            {
                services.Clear();
            }

            public bool Contains(ServiceDescriptor item)
            {
                return services.Contains(item);
            }

            public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
            {
                services.CopyTo(array, arrayIndex);
            }

            public IEnumerator<ServiceDescriptor> GetEnumerator()
            {
                return services.GetEnumerator();
            }

            public int IndexOf(ServiceDescriptor item)
            {
                return services.IndexOf(item);
            }

            public void Insert(int index, ServiceDescriptor item)
            {
                services.Insert(index, item);
            }

            public bool Remove(ServiceDescriptor item)
            {
                return services.Remove(item);
            }

            public void RemoveAt(int index)
            {
                services.RemoveAt(index);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return services.GetEnumerator();
            }

            #endregion
        }

        #endregion

    }
}