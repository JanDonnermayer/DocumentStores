using System;
using System.Threading.Tasks;
using DocumentStores.Primitives;
using DocumentStores.Internal;
using System.Collections.Generic;
using System.Threading;

namespace DocumentStores
{
    /// <summary/> 
    public sealed class JsonFileDocumentStore : FileDocumentStore<JsonFileHandling>
    {
        /// <summary/> 
        public JsonFileDocumentStore(string directory) : base(directory)
        {
        }
    }
}
