#nullable enable


using System;

namespace DocumentStores.Primitives
{
    public readonly struct DocumentAddress
    {
        public readonly DocumentRoute Route;
        public readonly DocumentKey Key;

        private DocumentAddress(DocumentRoute route, DocumentKey key)
        {
            Route = route;
            Key = key;
        }

        public static DocumentAddress Create(DocumentRoute route, DocumentKey key) => 
            new DocumentAddress(route, key);

        public static DocumentAddress Create(DocumentKey key) => 
            Create(DocumentRoute.Default, key);

        internal DocumentAddress MapRoute(Func<DocumentRoute, DocumentRoute> mapper) =>
            Create(mapper(Route), Key);

        internal DocumentAddress MapKey(Func<DocumentKey, DocumentKey> mapper) => 
            Create(Route, mapper(Key));


        #region Overrides
            
        public override string ToString() => 
            $"{nameof(Route)} : {Route}, {nameof(Key)} : {Key}";

        #endregion

        #region Operators

        public static implicit operator DocumentAddress(string key) => Create(key);


        #endregion        
    }

}