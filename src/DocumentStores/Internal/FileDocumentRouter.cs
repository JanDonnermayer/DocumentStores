#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using DocumentStores.Primitives;

namespace DocumentStores.Internal
{
    internal static class FileDocumentRouter
    {
        public static string ToPath(this DocumentRoute route) =>
            string.Join(
                separator.ToString(),
                route.Encode().Append("").ToArray());

        public static string ToPath(this DocumentAddress address) =>
            string.Join(
                separator.ToString(),
                address.Route.Encode().Append(address.Key.Encode().Value).ToArray());

        public static DocumentRoute GetRoute(string path)
        {
            if (string.IsNullOrEmpty(path))
                return DocumentRoute.Default;

            if (Path.IsPathRooted(path))
                throw new ArgumentException("Rooted paths are not suported!");

            return Path
                    .GetDirectoryName(path)
                    .Split(separator)
                    .ToRoute()
                    .Decode();
        }

        public static DocumentKey GetKey(string path)
        {
            return DocumentKey.Create(Path.GetFileName(path)).Decode();
        }

        public static DocumentAddress ToAddress(this DocumentRoute route, DocumentKey key) =>
            DocumentAddress.Create(route, key);

        private static DocumentRoute ToRoute(this IEnumerable<string> segments) =>
            DocumentRoute.Create(segments);

        private static DocumentKey Encode(this DocumentKey source) =>
            source.MapValue(CheckChars).MapValue(Encode);

        private static DocumentRoute Encode(this DocumentRoute source) =>
            source.MapSegments(CheckChars).MapSegments(Encode);

        private static DocumentRoute Decode(this DocumentRoute source) =>
            source.MapSegments(Decode);

        private static DocumentKey Decode(this DocumentKey source) =>
            source.MapValue(Decode);


        private static readonly char separator = Path.DirectorySeparatorChar;
        private static readonly char altSeparator = Path.AltDirectorySeparatorChar;

        // Map invalid filename chars to some weird unicode
        private static readonly IImmutableDictionary<char, char> _encodingMap =
            Path
                .GetInvalidFileNameChars()
                .Select((_, i) => new KeyValuePair<char, char>(_, (char)(i + 1700)))
                .ToImmutableDictionary();

        private static readonly IImmutableDictionary<char, char> _decodingMap =
            _encodingMap
                .Select(kvp => new KeyValuePair<char, char>(kvp.Value, kvp.Key))
                .ToImmutableDictionary();

        private static readonly Lazy<IImmutableDictionary<char, char>> encodingMap =
            new Lazy<IImmutableDictionary<char, char>>(() => _encodingMap);

        private static readonly Lazy<IImmutableDictionary<char, char>> decodingMap =
            new Lazy<IImmutableDictionary<char, char>>(() => _decodingMap);

        private static string Encode(string value) =>
            new string(value.Select(_ => encodingMap.Value.TryGetValue(_, out var v) ? v : _).ToArray());

        private static string Decode(string encodedValue) =>
            new string(encodedValue.Select(_ => decodingMap.Value.TryGetValue(_, out var v) ? v : _).ToArray());

        // Check whether key is null,
        // or contains anything from decoding map which would lead to collisions
        private static string CheckChars(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (key.Any(_decodingMap.Keys.Contains))
                throw new ArgumentException("Key contains invalid chars!", nameof(key));
            return key;
        }


    }




}