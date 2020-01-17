#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DocumentStores
{
    /// <InheritDoc/>
    public readonly struct DocumentRoute : IEquatable<DocumentRoute>
    {
        private readonly ImmutableArray<string> segments;

        /// <summary>
        /// The segments of the route.
        /// </summary>
        public IEnumerable<string> Segments => segments;

        private DocumentRoute(IEnumerable<string> segments) =>
            this.segments = (segments ?? throw new ArgumentNullException(nameof(segments)))
                .ToImmutableArray();

        /// <InheritDoc/>
        public static DocumentRoute Default => Create(Enumerable.Empty<string>());

        /// <InheritDoc/>
        public static DocumentRoute Create(IEnumerable<string> segments) =>
            new DocumentRoute(segments);

        /// <InheritDoc/>
        public static DocumentRoute Create(params string[] segments) =>
            new DocumentRoute(segments);

        /// <InheritDoc/>
        public DocumentRoute Prepend(DocumentRoute route) =>
            new DocumentRoute(route.segments.Concat(this.segments));

        /// <InheritDoc/>
        public DocumentRoute Append(DocumentRoute route) =>
            new DocumentRoute(this.segments.Concat(route.segments));

        /// <InheritDoc/>
        public DocumentRoute TrimLeft(DocumentRoute route) =>
            StartsWith(route) switch
            {
                true => Create(segments.Skip(route.Segments.Count())),
                false => this
            };

        internal DocumentRoute MapSegments(Func<string, string> mapper) =>
            new DocumentRoute(this.segments.Select(mapper));

        /// <InheritDoc/>
        public bool StartsWith(DocumentRoute route)
        {
            var segments = this.segments;
            var otherSegments = route.Segments;

            return (otherSegments.Count(), segments.Length) switch
            {
                (0, _) => true,
                (var x, var y) when x > y => false,
                _ => segments
                    .Zip(otherSegments, (x,y) => (x, y))
                    .All(z => EqualityComparer<string>.Default.Equals(z.x, z.y))
            };
        }

        #region "Override"

        /// <inheritdoc/>
        public bool Equals(DocumentRoute other) =>
            Enumerable.SequenceEqual(segments, other.segments);

        /// <inheritdoc/>
        public override bool Equals(object? obj) =>
            obj is DocumentRoute name &&
                Enumerable.SequenceEqual(segments, name.segments);

        private int GetSegmentsHash()
        {
            if (!segments.Any()) return 0;
            return segments
                .Select(s => s.GetHashCode())
                .Aggregate((x, y) => x ^ y);
        }

        /// <inheritdoc/>
        public override int GetHashCode() =>
            -312155673 + GetSegmentsHash();

        /// <inheritdoc/>
        public override string ToString() =>
           $"[{String.Join(", ", segments.ToArray())}]";

        #endregion

        #region  Operators

        /// <inheritdoc />
        public static bool operator ==(DocumentRoute left, DocumentRoute right)
        {
            return left.Equals(right);
        }

        /// <inheritdoc />
        public static bool operator !=(DocumentRoute left, DocumentRoute right)
        {
            return !(left == right);
        }

        #endregion
    }
}