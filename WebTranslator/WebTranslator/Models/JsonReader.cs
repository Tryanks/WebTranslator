using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WebTranslator.Models;

public partial class JsonReader
{
    private string OriginalText { get; set; }
    private List<string> JsonSplits { get; set; }
    public readonly List<TransElement> ElementList = new();

    public JsonReader(string enText, string zhText = "")
    {
        OriginalText = enText;
        var matches = jsonRegex.Matches(enText);
        foreach (Match match in matches)
        {
            var key = match.Groups[1].Value;
            var value = match.Groups[2].Value;
            var e = new TransElement(key, value);
            ElementList.Add(e);
        }
        if (!string.IsNullOrEmpty(zhText)) SetZhText(zhText);
        var en = jsonRegex.Replace(enText, "||||Split|||");
        JsonSplits = en.Split("||||Split|||").ToList();
    }
    
    public void SetZhText(string zhText)
    {
        if (string.IsNullOrEmpty(zhText)) return;
        var matches = jsonRegex.Matches(zhText);
        var zhDict = new Dictionary<string, string>();
        foreach (Match match in matches)
        {
            var key = match.Groups[1].Value;
            var value = match.Groups[2].Value;
            zhDict.TryAdd(key, value);
        }
        ElementList.ForEach(e =>
        {
            if (zhDict.TryGetValue(e.Key, out var value))
                e.ZhValue = value;
        });
        ElementList.ForEach(e =>
        {
            if (string.IsNullOrEmpty(e.ZhValue))
                e.ZhValue = e.EnValue;
        });
    }

    public string GetZhText()
    {
        var text = new StringBuilder();
        for (var i = 0; i < ElementList.Count; i++)
        {
            text.Append(JsonSplits[i]);
            text.Append(ElementList[i].KeyValue());
        }
        text.Append(JsonSplits[^1]);
        return text.ToString();
    }

    public void SetValue(string key, string value)
    {
        // if (string.IsNullOrEmpty(key)) return;
        // if (string.IsNullOrEmpty(value)) return;
        // var reg1 = SourceRegex(key);
        // var originalValue = reg1.Match(TransText).Groups[1].Value;
        // if (originalValue == value) return;
        // var reg2 = TransRegex(key, value);
        // TransText = reg1.Replace(TransText, reg2);
    }

    private readonly Regex jsonRegex = MyRegex();
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

public class TransElement
{
    public TransElement(string k, string en, string zh = "")
    {
        Key = k;
        EnValue = en;
        ZhValue = zh;
    }
    public string Key { get; set; }
    public string EnValue { get; set; }
    public string ZhValue { get; set; }

    public string KeyValue()
    {
        var value = string.IsNullOrEmpty(ZhValue) ? EnValue : ZhValue;
        return @$"""{Key}"": ""{value}""";
    }
}