#nullable enable


using System;
using System.Collections.Generic;

namespace DocumentStores.Primitives
{
    /// <InheritDoc/>
    public readonly struct DocumentAddress
    {
        /// <InheritDoc/>
        public readonly DocumentRoute Route;

        /// <InheritDoc/>
        public readonly DocumentKey Key;

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

        internal DocumentAddress MapRoute(Func<DocumentRoute, DocumentRoute> mapper) =>
            Create(mapper(Route), Key);

        internal DocumentAddress MapKey(Func<DocumentKey, DocumentKey> mapper) => 
            Create(Route, mapper(Key));


        #region Overrides
            
        /// <InheritDoc/>
        public override string ToString() => 
            $"{nameof(Route)} : {Route}, {nameof(Key)} : {Key}";

        /// <InheritDoc/>
        public override bool Equals(object? obj)
        {
            return obj is DocumentAddress address &&
                   EqualityComparer<DocumentRoute>.Default.Equals(Route, address.Route) &&
                   EqualityComparer<DocumentKey>.Default.Equals(Key, address.Key);
        }

        /// <InheritDoc/>
        public override int GetHashCode()
        {
            int hashCode = 565030266;
            hashCode = hashCode * -1521134295 + Route.GetHashCode();
            hashCode = hashCode * -1521134295 + Key.GetHashCode();
            return hashCode;
        }

        #endregion

        #region Operators

        /// <InheritDoc/>
        public static implicit operator DocumentAddress(string key) => Create(key);


        #endregion        
    }

}