using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WebTranslator.Models;

public static partial class FormatStringHelper
{
    private static readonly Regex FormatRegex = MyRegex();

    public static FormatMode DetermineFormatMode(string input)
    {
        if (string.IsNullOrEmpty(input))
            return FormatMode.None;

        var matches = FormatRegex.Matches(input);
        if (matches.Count == 0)
            return FormatMode.None;

        foreach (Match match in matches)
            if (match.Value.Contains('$'))
                return FormatMode.Sorted;

        return FormatMode.Format;
    }

    public static List<string> ExtractFormatStrings(string input)
    {
        var result = new List<string>();

        if (string.IsNullOrEmpty(input))
            return result;

        var matches = FormatRegex.Matches(input);
        foreach (Match match in matches) result.Add(match.Value);

        return result;
    }

    [GeneratedRegex(@"%(\d+\$)?[a-hs%]", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}

public enum FormatMode
{
    None,
    Format,
    Sorted
}