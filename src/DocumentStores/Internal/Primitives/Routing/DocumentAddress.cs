#nullable enable


using System;
using System.Collections.Generic;

namespace DocumentStores
{
    /// <InheritDoc/>
    public readonly struct DocumentAddress : IEquatable<DocumentAddress>
    {
        /// <InheritDoc/>
        public DocumentRoute Route { get; }

        /// <InheritDoc/>
        public DocumentKey Key { get; }

        private DocumentAddress(DocumentRoute route, DocumentKey key)
        {
            Route = route;
            Key = key;
        }

        /// <InheritDoc/>
        public static DocumentAddress Create(DocumentRoute route, DocumentKey key) =>
            new DocumentAddress(route, key);

        /// <InheritDoc/>
        public static DocumentAddress Create(DocumentKey key) =>
            Create(DocumentRoute.Default, key);

        /// <InheritDoc/>
        public static DocumentAddress FromString(string key) =>
            Create(DocumentRoute.Default, key);

        internal DocumentAddress MapRoute(Func<DocumentRoute, DocumentRoute> mapper) =>
            Create(mapper(Route), Key);

        internal DocumentAddress MapKey(Func<DocumentKey, DocumentKey> mapper) =>
            Create(Route, mapper(Key));

        #region Overrides

        /// <InheritDoc/>
        public override string ToString() =>
            $"{nameof(Route)} : {Route}, {nameof(Key)} : {Key}";

        /// <InheritDoc/>
        public bool Equals(DocumentAddress other) =>
            EqualityComparer<DocumentRoute>.Default.Equals(Route, other.Route) &&
            EqualityComparer<DocumentKey>.Default.Equals(Key, other.Key);

        /// <InheritDoc/>
        public override bool Equals(object? obj) =>
            obj is DocumentAddress address && Equals(address);

        /// <InheritDoc/>
        public override int GetHashCode()
        {
            int hashCode = 565030266;
            hashCode = (hashCode * -1521134295) + Route.GetHashCode();
            hashCode = (hashCode * -1521134295) + Key.GetHashCode();
            return hashCode;
        }

        #endregion

        #region Operators

        /// <InheritDoc/>
        public static implicit operator DocumentAddress(string key) => Create(key);

        ///<inheritdoc />
        public static bool operator ==(DocumentAddress left, DocumentAddress right)
        {
            return left.Equals(right);
        }

        ///<inheritdoc />
        public static bool operator !=(DocumentAddress left, DocumentAddress right)
        {
            return !(left == right);
        }

        #endregion
    }

}