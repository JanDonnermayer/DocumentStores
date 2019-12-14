#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using DocumentStores.Primitives;

namespace DocumentStores.Internal
{
    internal static class DocumentAddressBuilder
    {

        public static string ToPath(this DocumentRoute route) =>
            string.Join(
                separator.ToString(), 
                route.Encode().Segments);

        public static string ToPath(this DocumentAddress address)
        {
            return string.Join(
                separator.ToString(),
                address.Route.ToPath(),
                address.Key.Encode());
        }

        public static DocumentRoute GetRoute(string path) => 
            path.Split(separator).ToRoute().Decode();

        public static DocumentAddress GetAddress(string path) => 
            GetRoute(path).ToAddress(Path.GetFileName(path)).Decode();

        public static DocumentAddress ToAddress(this DocumentRoute route, DocumentKey key) =>
            DocumentAddress.Create(route, key);

        private static DocumentRoute ToRoute(this IEnumerable<string> segments) =>
            DocumentRoute.Create(segments);

        private static DocumentKey Encode(this DocumentKey source) =>
            source.Map(CheckChars).Map(Encode);

        private static DocumentRoute Encode(this DocumentRoute source) =>
            source.Map(CheckChars).Map(Encode);

        private static DocumentAddress Decode(this DocumentAddress source) =>
            source.Map(Decode);

        private static DocumentRoute Decode(this DocumentRoute source) =>
            source.Map(Decode);

        private static readonly char separator = Path.DirectorySeparatorChar;

        // Map invalid filename chars to some weird unicode
        private static readonly ImmutableDictionary<char, char> encodingMap =
            Path
                .GetInvalidFileNameChars()
                .Select((_, i) => new KeyValuePair<char, char>(_, (char)(i + 2000)))
                .ToImmutableDictionary();

        private static readonly IImmutableDictionary<char, char> decodingMap =
            encodingMap
                .Select(kvp => new KeyValuePair<char, char>(kvp.Value, kvp.Key))
                .ToImmutableDictionary();

        private static string Encode(string key) =>
            new string(key.Select(_ => encodingMap.TryGetValue(_, out var v) ? v : _).ToArray());

        private static string Decode(string encodedKey) =>
            new string(encodedKey.Select(_ => decodingMap.TryGetValue(_, out var v) ? v : _).ToArray());

        // Check whether key is null,
        // or contains anything from decoding map which would lead to collisions
        private static string CheckChars(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (key.Any(decodingMap.Keys.Contains))
                throw new ArgumentException("Key contains invalid chars!", nameof(key));
            return key;
        }


    }




}