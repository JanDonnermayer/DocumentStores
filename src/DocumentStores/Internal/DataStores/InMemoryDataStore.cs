using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace DocumentStores.Internal
{
    internal class InMemoryDataStore : IDataStore
    {
        private ImmutableDictionary<DocumentAddress, DataContainer> store =
            ImmutableDictionary<DocumentAddress, DataContainer>.Empty;

        void IDataStore.Delete(DocumentAddress address) =>
            ImmutableInterlocked.TryRemove(ref store, address, out var _);

        bool IDataStore.Exists(DocumentAddress address) =>
            store.ContainsKey(address);

        IEnumerable<DocumentAddress> IDataStore.GetAddresses(
            DocumentRoute route, DocumentSearchOption options) =>
                options switch
                {
                    DocumentSearchOption.AllLevels =>
                        store.Keys.Where(r => r.Route.StartsWith(route)),
                    DocumentSearchOption.TopLevelOnly =>
                        store.Keys.Where(r => r.Route.Equals(route)),
                    _ => throw new InvalidDocumentSearchOptionsException(options)
                };

        Stream IDataStore.GetReadStream(DocumentAddress address) =>
            (store.TryGetValue(address, out var container)) switch
            {
                true => container.GetReadStream(),
                false => throw new DocumentMissingException(address)
            };

        Stream IDataStore.GetWriteStream(DocumentAddress address) =>
            ImmutableInterlocked
                .GetOrAdd(
                    location: ref store,
                    key: address,
                    valueFactory: _ => new DataContainer())
                .GetWriteStream();

        void IDataStore.Clear() =>
            store = store.Clear();

        #region  Private Types

        private class DataContainer
        {
            private byte[]? data;

            private void SetData(byte[] _data) =>
                this.data = _data;

            public Stream GetReadStream()
            {
                if (data is null) data = Array.Empty<byte>();
                return new MemoryStream(data, writable: false);
            }

            public Stream GetWriteStream()
            {
                var stream = new ObservableMemoryStream();
                stream.OnDispose().Subscribe(SetData);
                return stream;
            }
        }

        #endregion

    }

}
