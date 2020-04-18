using System;
using DocumentStores.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentStores
{
    /// <inheritdoc/>
    public static class IDocumentTopicBuilderExtensions
    {
        ///<summary>
        /// Adds an <see cref="IDocumentTopic{TData}"/> with the specified <paramref name="route"/>
        /// to the <see cref="IServiceCollection"/>.
        ///</summary>
        public static IDocumentTopicBuilder AddDocumentTopic<T>(
            this IDocumentTopicBuilder builder,
            DocumentRoute route
        ) where T : class
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            builder
                 .Services
                 .AddSingleton<IDocumentTopic<T>>(
                     sp => new DocumentTopic<T>(
                         source: sp.GetRequiredService<IDocumentStore>(),
                         route: route
                     )
                 );

            return builder;
        }

        ///<summary>
        /// Adds an <see cref="IDocumentTopic{TData}"/> with default route
        /// to the <see cref="IServiceCollection"/>.
        ///</summary>
        public static IDocumentTopicBuilder AddTopic<T>(
            this IDocumentTopicBuilder builder
        ) where T : class
        {
            return builder.AddDocumentTopic<T>(DocumentRoute.Default);
        }
    }
}