using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentStores.Internal
{
    internal class ObservableDocumentStoreBuilder : IServiceCollection, IObservableDocumentStoreBuilder
    {
        private readonly IServiceCollection services;

        public ObservableDocumentStoreBuilder(IServiceCollection services) =>
            this.services = services ?? throw new ArgumentNullException(nameof(services));

        public IObservableDocumentStoreBuilder WithObservableOn<TData>() where TData : class
        {
            this.AddSingleton<IObservableDocumentStore<TData>>(sp => new ObservableDocumentStore<TData>(sp.GetRequiredService<IDocumentStore>()));
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

}