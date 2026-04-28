using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Platform;
using Newtonsoft.Json.Linq;

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
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        if (!File.Exists(source.LocalPath)) return null;
        return File.ReadAllText(source.LocalPath);
    }

    private static Dictionary<string, List<string>> Parse(string json)
    {
        var result = new Dictionary<string, List<string>>();
        var doc = JObject.Parse(json);
        foreach (var property in doc.Properties())
        {
            var values = property.Value switch
            {
                JArray array => array.Values<string>().OfType<string>().Where(x => !string.IsNullOrWhiteSpace(x)).ToList(),
                JValue value when value.Type == JTokenType.String => [value.Value<string>()!],
                _ => []
            };

            if (values.Count > 0) result[property.Name] = values;
        }

        return result;
    }
}
