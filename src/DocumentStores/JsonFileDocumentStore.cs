using System;
using System.Threading.Tasks;
using DocumentStores.Primitives;
using DocumentStores.Internal;
using System.Collections.Generic;
using System.Threading;

namespace DocumentStores
{
    public sealed class JsonFileDocumentStore : FileDocumentStore<JsonHandling>
    {
        public JsonFileDocumentStore(string directory) : base(directory)
        {
        }
    }
}
