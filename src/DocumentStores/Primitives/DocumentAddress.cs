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

        public DocumentAddress Map(Func<string, string> mapper) => 
            Create(Route.Map(mapper), Key.Map(mapper));
        
    }

}