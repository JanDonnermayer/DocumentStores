#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DocumentStores.Primitives
{

    public readonly struct DocumentRoute
    {
        private readonly ImmutableArray<string> segments;
        public readonly IEnumerable<string> Segments => segments;

        private DocumentRoute(IEnumerable<string> segments) =>
            this.segments = (segments ?? throw new ArgumentNullException(nameof(segments)))
                .ToImmutableArray();

        public static DocumentRoute Default => Create(Enumerable.Empty<string>());

        public static DocumentRoute Create(IEnumerable<string> segments) =>
            new DocumentRoute(segments);

        public static DocumentRoute Create(params string[] segments) =>
            new DocumentRoute(segments);


        internal DocumentRoute Prepend(DocumentRoute route) =>
            new DocumentRoute(route.segments.Concat(this.segments));

        internal DocumentRoute Append(DocumentRoute route) =>
            new DocumentRoute(this.segments.Concat(route.segments));

        internal DocumentRoute MapSegments(Func<string, string> mapper) =>
            new DocumentRoute(this.segments.Select(mapper));


        #region "Override"

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is DocumentRoute name &&
                Enumerable.SequenceEqual(segments, name.segments);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return -312155673 + segments
                .Select(s => s.GetHashCode())
                .Aggregate((x, y) => x ^ y);
        }

        /// <inheritdoc/>
        public override string ToString() =>
           $"[{String.Join(", ", Segments.ToArray())}]";

        #endregion

        /// <inheritdoc/>
        public static implicit operator DocumentRoute(string segment) => DocumentRoute.Create(segment);
      

    }

}