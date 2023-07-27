using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WebTranslator.Models;

public partial class JsonReader
{
    private string OriginalText { get; set; }
    public readonly List<List<string>> Dict = new();

    public JsonReader(string text)
    {
        OriginalText = text;
        var matches = jsonRegex.Matches(text);
        foreach (Match match in matches)
        {
            var key = match.Groups[1].Value;
            var value = match.Groups[2].Value;
            Dict.Add(new List<string> {key, value});
        }
    }

    public void SetValue(string key, string value)
    {
        if (string.IsNullOrEmpty(key)) return;
        if (string.IsNullOrEmpty(value)) return;
        var reg1 = SourceRegex(key);
        var originalValue = reg1.Match(OriginalText).Groups[1].Value;
        if (originalValue == value) return;
        var reg2 = TransRegex(key, value);
        OriginalText = reg1.Replace(OriginalText, reg2);
    }

    private Regex jsonRegex = MyRegex();
    [GeneratedRegex(""""
"((?:[^"\\]|\\.)*)"\:\s*((\[[^\[^\]]*\]|"((?:[^"\\]|\\.)*)"))
"""")]
    private static partial Regex MyRegex();

    private Regex SourceRegex(string key)
    {
        return new Regex($""""
"{key}"\:\s*((\[[^\[^\]]*\]|"((?:[^"\\]|\\.)*)"))
"""");
    }
    
    private string TransRegex(string key, string value)
    {
        return $""""
"{key}"\:\s*{value}
"""";
    }
}

public partial class JsonReader
{
    
}