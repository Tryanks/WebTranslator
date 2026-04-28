using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using Avalonia.Platform;

namespace WebTranslator.Services;

public static class DictionaryService
{
    private static Dictionary<string, List<string>>? Entries { get; set; }

    public static IReadOnlyList<string> GetTranslations(string originalText)
    {
        if (string.IsNullOrWhiteSpace(originalText)) return [];
        Entries ??= LoadEntries();
        return Entries.TryGetValue(originalText, out var values) ? values : [];
    }

    private static Dictionary<string, List<string>> LoadEntries()
    {
        foreach (var source in EnumerateSources())
        {
            try
            {
                var json = ReadSource(source);
                if (string.IsNullOrWhiteSpace(json)) continue;
                return Parse(json);
            }
            catch
            {
                // Dictionaries are optional user data; ignore unavailable or malformed files.
            }
        }

        return new Dictionary<string, List<string>>();
    }

    private static IEnumerable<Uri> EnumerateSources()
    {
        yield return new Uri("avares://WebTranslator/Assets/dict.json.br");
        yield return new Uri("avares://WebTranslator/Assets/dict.json");
        yield return new Uri(Path.Combine(AppContext.BaseDirectory, "dict.json"));
        yield return new Uri(Path.Combine(AppContext.BaseDirectory, "Dict-Mini.json"));
        yield return new Uri(Path.Combine(Environment.CurrentDirectory, "dict.json"));
        yield return new Uri(Path.Combine(Environment.CurrentDirectory, "Dict-Mini.json"));
    }

    private static string? ReadSource(Uri source)
    {
        if (source.Scheme == "avares")
        {
            if (!AssetLoader.Exists(source)) return null;
            using var stream = AssetLoader.Open(source);
            if (source.AbsolutePath.EndsWith(".br", StringComparison.OrdinalIgnoreCase))
            {
                using var brotli = new BrotliStream(stream, CompressionMode.Decompress);
                using var compressedReader = new StreamReader(brotli);
                return compressedReader.ReadToEnd();
            }

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        if (!File.Exists(source.LocalPath)) return null;
        return File.ReadAllText(source.LocalPath);
    }

    private static Dictionary<string, List<string>> Parse(string json)
    {
        var result = new Dictionary<string, List<string>>();
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.ValueKind != JsonValueKind.Object) return result;

        foreach (var property in doc.RootElement.EnumerateObject())
        {
            var values = property.Value.ValueKind switch
            {
                JsonValueKind.Array => property.Value.EnumerateArray()
                    .Where(x => x.ValueKind == JsonValueKind.String)
                    .Select(x => x.GetString())
                    .OfType<string>()
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList(),
                JsonValueKind.String => [property.Value.GetString()!],
                _ => []
            };

            if (values.Count > 0) result[property.Name] = values;
        }

        return result;
    }
}
