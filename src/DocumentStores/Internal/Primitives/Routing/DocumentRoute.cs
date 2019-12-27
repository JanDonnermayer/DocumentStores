#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DocumentStores.Primitives
{
    /// <InheritDoc/>
    public readonly struct DocumentRoute : IEnumerable<string>
    {
        private readonly ImmutableArray<string> segments;

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


        internal DocumentRoute Prepend(DocumentRoute route) =>
            new DocumentRoute(route.segments.Concat(this.segments));

        internal DocumentRoute Append(DocumentRoute route) =>
            new DocumentRoute(this.segments.Concat(route.segments));

        internal DocumentRoute TrimLeft(DocumentRoute route) =>
            StartsWith(route) switch
            {
                true => Create(segments.Skip(route.Count())),
                false => this
            };

        internal DocumentRoute MapSegments(Func<string, string> mapper) =>
            new DocumentRoute(this.segments.Select(mapper));

        /// <InheritDoc/>
        public bool StartsWith(DocumentRoute route) => 
            (route.Count(), segments.Count()) switch
            {
                (0, 0) => true,
                (var x, var y) when x > y => false, 
                _ => Enumerable.SequenceEqual(this.Take(route.Count()), route)
            };
            

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


        /// <inheritdoc/>
        public IEnumerator<string> GetEnumerator() => 
            ((IEnumerable<string>)segments).GetEnumerator();


        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => 
            ((IEnumerable<string>)segments).GetEnumerator();

        #endregion

        /// <inheritdoc/>
        public static implicit operator DocumentRoute(string segment) => DocumentRoute.Create(segment);
      

    }

}