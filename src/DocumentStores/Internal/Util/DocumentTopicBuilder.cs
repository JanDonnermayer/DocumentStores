using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace DocumentStores.Internal
{
    /// <inheritdoc />
    internal class DocumentTopicBuilder : IDocumentTopicBuilder
    {
        public DocumentTopicBuilder(IServiceCollection collection)
        {
            if (collection is null)
                throw new ArgumentNullException(nameof(collection));
            this.Services = collection;
        }

        public IServiceCollection Services { get; }
    }
}