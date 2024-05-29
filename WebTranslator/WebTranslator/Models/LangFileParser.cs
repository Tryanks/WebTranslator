using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WebTranslator.Models;

public static partial class LangFileParser
{
    private static readonly Regex JsonKvPairRegex = MyRegex();

    private static readonly char[] LangSeparator = ['\r', '\n'];

    public static LangFileParserResult Parse(string text, LangFormat format)
    {
        var sd = new SortedDictionary<string, LangFileDictionaryResult>();
        var templatedText = format switch
        {
            LangFormat.Lang => LangParse(sd, text),
            LangFormat.Json => Json5Parse(sd, text),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };


        return new LangFileParserResult(sd, templatedText);
    }

    private static string Json5Parse(SortedDictionary<string, LangFileDictionaryResult> sd, string text)
    {
        var matches = JsonKvPairRegex.Matches(text);
        var i = 0;
        foreach (Match match in matches)
        {
            var key = match.Groups[1].Value;
            var value = match.Groups[2].Value;
            if (sd.ContainsKey(key)) sd.Remove(key);
            var template = $"\"|WebTranslator|Content{i++}|WebTranslator|\"";
            sd.Add(key, new LangFileDictionaryResult(value.ReplaceJson5(), template));
            text = text.ReplaceOnce(value, template);
        }

        return text;
    }

    private static string LangParse(SortedDictionary<string, LangFileDictionaryResult> sd, string text)
    {
        var lines = text.Split(LangSeparator, StringSplitOptions.RemoveEmptyEntries);
        var i = 0;
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (trimmedLine.StartsWith('#') || trimmedLine.StartsWith("//") ||
                string.IsNullOrWhiteSpace(trimmedLine)) continue;

            var separatorIndex = trimmedLine.IndexOf('=');
            if (separatorIndex == -1) continue;

            var key = trimmedLine[..separatorIndex].Trim();
            var value = trimmedLine[(separatorIndex + 1)..].Trim();
            if (sd.ContainsKey(key)) sd.Remove(key);
            var template = $"|WebTranslator|Content{i++}|WebTranslator|";
            sd.Add(key, new LangFileDictionaryResult(value, template));
            text = text.ReplaceOnce(value, template);
        }

        return text;
    }

    [GeneratedRegex("""
                    "((?:[^"\\]|\\.)*)"\:\s*((\[[^\[^\]]*\]|"((?:[^"\\]|\\.)*)"))
                    """)]
    private static partial Regex MyRegex();
}

public readonly struct LangFileParserResult(SortedDictionary<string, LangFileDictionaryResult> sd, string text = "")
{
    public readonly SortedDictionary<string, LangFileDictionaryResult> Dictionary = sd;
    public string Template { get; } = text;
}

public readonly struct LangFileDictionaryResult(string text, string template)
{
    public string Text { get; } = text;
    public string Template { get; } = template;
}