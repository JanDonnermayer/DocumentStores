#nullable enable


using System;

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

        #endregion

        #region Operators

        /// <InheritDoc/>
        public static implicit operator DocumentAddress(string key) => Create(key);


        #endregion        
    }

}