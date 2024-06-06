using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WebTranslator.Models;

public static partial class LangFileParser
{
    private static readonly Regex JsonKvPairRegex = MyRegex();

    private static readonly char[] LangSeparator = ['\r', '\n'];

    public static LangFileParserResult Parse(string text, LangFormat format)
    {
        var sd = new Dictionary<string, LangFileDictionaryResult>();
        var keys = new List<string>();
        var templatedText = format switch
        {
            LangFormat.Lang => LangParse(sd, keys, text),
            LangFormat.Json => Json5Parse(sd, keys, text),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };

        return new LangFileParserResult(sd, keys, templatedText);
    }

    private static string Json5Parse(Dictionary<string, LangFileDictionaryResult> sd, List<string> keys, string text)
    {
        text = text.ReplaceJson5();
        var matches = JsonKvPairRegex.Matches(text);
        var textBuilder = new StringBuilder(text);
        var generator = new Helper.WebTranslatorTemplate();
        foreach (var match in matches.Reverse())
        {
            var key = match.Groups[1].Value;
            var value = match.Groups[2].Value;
            if (keys.Contains(key)) continue;
            keys.Insert(0, key);
            var template = generator.Get();
            sd[key] = new LangFileDictionaryResult(value, template);
            textBuilder.ReplaceMatchGroup(match.Groups[2], template);
        }

        return textBuilder.ToString();
    }

    private static string LangParse(Dictionary<string, LangFileDictionaryResult> sd, List<string> keys, string text)
    {
        var lines = text.Split(LangSeparator, StringSplitOptions.RemoveEmptyEntries);
        var textBuilder = new StringBuilder(text);
        var generator = new Helper.WebTranslatorTemplate();
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (trimmedLine.StartsWith('#') || trimmedLine.StartsWith("//") ||
                string.IsNullOrWhiteSpace(trimmedLine)) continue;

            var separatorIndex = trimmedLine.IndexOf('=');
            if (separatorIndex == -1) continue;

            var key = trimmedLine[..separatorIndex].Trim();
            var value = trimmedLine[(separatorIndex + 1)..].Trim();
            if (keys.Contains(key)) continue;
            keys.Add(key);
            var template = generator.Get();
            sd[key] = new LangFileDictionaryResult(value, template);
            textBuilder.ReplaceOnce(value, template);
        }

        return textBuilder.ToString();
    }

    [GeneratedRegex("""
                    "((?:[^"\\]|\\.)*)"\:\s*((\[[^\[^\]]*\]|"((?:[^"\\]|\\.)*)"))
                    """)]
    private static partial Regex MyRegex();
}

public readonly struct LangFileParserResult(
    Dictionary<string, LangFileDictionaryResult> sd,
    List<string> keys,
    string text = "")
{
    public readonly Dictionary<string, LangFileDictionaryResult> Dictionary = sd;
    public List<string> Keys { get; } = keys;
    public string Template { get; } = text;
}

public readonly struct LangFileDictionaryResult(string text, string template)
{
    public string Text { get; } = text;
    public string Template { get; } = template;
}